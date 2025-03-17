import "dart:convert";

import "package:camera/camera.dart";
import "package:flutter/material.dart";
import "package:flutter/services.dart";
import "package:http/http.dart" as http;
import "package:image/image.dart" as img;
import "package:shared_preferences/shared_preferences.dart";
import "package:wakelock_plus/wakelock_plus.dart";
import "package:web_socket_channel/io.dart";

class CameraScreen extends StatefulWidget {
  @override
  _CameraScreenState createState() => _CameraScreenState();
}

class _CameraScreenState extends State<CameraScreen> {
  CameraController? _cameraController;
  String? token;
  List<dynamic> _buildings = [];
  int? selectedBuildingId;
  List<dynamic> _rooms = [];
  int? selectedRoomId;
  IOWebSocketChannel? _channel;
  bool isStreaming = false;
  int cameraId = 0;

  @override
  void initState() {
    super.initState();
    _initializeCamera();
    WakelockPlus.enable();
    _fetchBuildings();
  }

  Future<void> _initializeCamera() async {
    final cameras = await availableCameras();
    final backCamera = cameras.firstWhere(
      (camera) => camera.lensDirection == CameraLensDirection.back,
    );

    _cameraController = CameraController(
      backCamera,
      ResolutionPreset.medium,
      enableAudio: true,
    );

    await _cameraController!.initialize();
    setState(() {});
  }

  Future<void> _fetchBuildings() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    token = prefs.getString("jwt_token");

    final response = await http.get(
      //Uri.parse("http://10.0.2.2:5050/buildings"),
      Uri.parse("http://192.168.8.29:5050/buildings"),
      headers: {
        "Authorization": "Bearer $token",
        "Content-Type": "application/json",
      },
    );

    setState(() {
      _buildings = json.decode(response.body);
      selectedBuildingId = _buildings.isNotEmpty ? _buildings[0]["id"] : null;
    });
    await _fetchRooms(selectedBuildingId!);
  }

  Future<void> _fetchRooms(int buildingId) async {
    final response = await http.get(
      //Uri.parse("http://10.0.2.2:5050/buildings/$buildingId/rooms"),
      Uri.parse("http://192.168.8.29:5050/buildings/$buildingId/rooms"),
      headers: {
        "Authorization": "Bearer $token",
        "Content-Type": "application/json",
      },
    );

    setState(() {
      _rooms = json.decode(response.body);
      selectedRoomId = _rooms.isNotEmpty ? _rooms[0]["id"] : null;
    });
  }

  void _startStreaming() {
    if (isStreaming) return;

    if (_cameraController == null || !_cameraController!.value.isInitialized) {
      return;
    }

    setState(() {
      isStreaming = true;
    });

    //_channel = IOWebSocketChannel.connect("ws://10.0.2.2:5050/send");
    _channel = IOWebSocketChannel.connect("ws://192.168.8.29:5050/send");

    int lastFrameTime = 0;

    _channel!.sink.add(
      jsonEncode({
        "token": token,
        "building_id": selectedBuildingId,
        "room_id": selectedRoomId,
        "camera_id": cameraId,
      }),
    );

    _cameraController!.startImageStream((CameraImage image) {
      if (!isStreaming) return;

      int now = DateTime.now().millisecondsSinceEpoch;

      if (now - lastFrameTime < 50) return;
      lastFrameTime = now;

      Uint8List imageBytes = _convertImageToBytes(image);

      _channel!.sink.add(imageBytes);
    });
  }

  void _stopStreaming() {
    if (!isStreaming) return;

    setState(() {
      isStreaming = false;
    });

    _cameraController!.stopImageStream();
    _channel?.sink.close();
    _channel = null;
  }

  Uint8List _convertImageToBytes(CameraImage image) {
    try {
      img.Image convertedImage = _convertYUV420ToImage(image);

      return Uint8List.fromList(img.encodeJpg(convertedImage));
    } catch (e) {
      return Uint8List(0);
    }
  }

  img.Image _convertYUV420ToImage(CameraImage image) {
    final int width = image.width;
    final int height = image.height;
    final int uvRowStride = image.planes[1].bytesPerRow;
    final int uvPixelStride = image.planes[1].bytesPerPixel ?? 1;

    img.Image imgBuffer = img.Image(width: width, height: height);

    final Uint8List yBuffer = image.planes[0].bytes;
    final Uint8List uBuffer = image.planes[1].bytes;
    final Uint8List vBuffer = image.planes[2].bytes;

    for (int y = 0; y < height; y++) {
      for (int x = 0; x < width; x++) {
        final int uvIndex = uvPixelStride * (x ~/ 2) + uvRowStride * (y ~/ 2);
        final int yp = y * width + x;

        final int yValue = yBuffer[yp];
        final int uValue = uBuffer[uvIndex];
        final int vValue = vBuffer[uvIndex];

        int r = (yValue + 1.370705 * (vValue - 128)).toInt();
        int g =
            (yValue - 0.698001 * (vValue - 128) - 0.337633 * (uValue - 128))
                .toInt();
        int b = (yValue + 1.732446 * (uValue - 128)).toInt();

        imgBuffer.setPixelRgb(
          x,
          y,
          r.clamp(0, 255),
          g.clamp(0, 255),
          b.clamp(0, 255),
        );
      }
    }
    return imgBuffer;
  }

  @override
  void dispose() {
    _cameraController?.dispose();
    WakelockPlus.disable();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            _cameraController == null || !_cameraController!.value.isInitialized
                ? CircularProgressIndicator()
                : CameraPreview(_cameraController!),
            _buildings.isEmpty
                ? Center(child: Text("No buildings found"))
                : Column(
                  children: [
                    DropdownButton<int>(
                      value: selectedBuildingId,
                      onChanged: (int? newValue) {
                        setState(() {
                          selectedBuildingId = newValue;
                        });
                        _fetchRooms(newValue!);
                      },
                      items:
                          _buildings.map<DropdownMenuItem<int>>((building) {
                            return DropdownMenuItem<int>(
                              value: building["id"],
                              child: Text(building["name"]),
                            );
                          }).toList(),
                    ),
                    _rooms.isEmpty
                        ? Center(child: Text("No rooms found"))
                        : DropdownButton<int>(
                          value: selectedRoomId,
                          onChanged: (int? newValue) {
                            setState(() {
                              selectedRoomId = newValue;
                            });
                            _fetchRooms(newValue!);
                          },
                          items:
                              _rooms.map<DropdownMenuItem<int>>((room) {
                                return DropdownMenuItem<int>(
                                  value: room["id"],
                                  child: Text(room["name"]),
                                );
                              }).toList(),
                        ),
                    DropdownButton<int>(
                      value: cameraId,
                      items: List.generate(5, (index) {
                        return DropdownMenuItem(
                          value: index,
                          child: Text(index.toString()),
                        );
                      }),
                      onChanged: (int? newValue) {
                        setState(() {
                          cameraId = newValue!;
                        });
                      },
                    ),
                    !isStreaming
                        ? ElevatedButton(
                          onPressed: _startStreaming,
                          child: Text("Start"),
                        )
                        : ElevatedButton(
                          onPressed: _stopStreaming,
                          child: Text("Stop"),
                        ),
                  ],
                ),
          ],
        ),
      ),
    );
  }
}
