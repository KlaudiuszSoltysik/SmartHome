import 'dart:convert';

import "package:camera/camera.dart";
import "package:flutter/material.dart";
import 'package:flutter/services.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class ScanScreen extends StatefulWidget {
  const ScanScreen({super.key});

  @override
  State<ScanScreen> createState() => _ScanScreenState();
}

class _ScanScreenState extends State<ScanScreen> {
  static const platform = MethodChannel('com.mobile_application/channel');
  CameraController? _cameraController;
  bool _isRecording = false;
  XFile? _videoFile;
  List<String> frames = [];
  int counter = 10;
  bool loader = false;

  @override
  void initState() {
    super.initState();
    _initializeCamera();
  }

  Future<void> _initializeCamera() async {
    final cameras = await availableCameras();
    final camera = cameras.firstWhere(
      (camera) => camera.lensDirection == CameraLensDirection.front,
    );

    _cameraController = CameraController(
      camera,
      ResolutionPreset.high,
      enableAudio: false,
    );

    await _cameraController!.initialize();
    setState(() {});
  }

  Future<void> _startRecording() async {
    if (_cameraController == null || !_cameraController!.value.isInitialized) {
      return;
    }

    setState(() => _isRecording = true);

    await _cameraController!.startVideoRecording();

    for (int i = 0; i < 10; i++) {
      setState(() => counter--);
      await Future.delayed(Duration(seconds: 1));
    }

    setState(() {
      _isRecording = false;
      counter = 10;
      loader = true;
    });

    _videoFile = await _cameraController!.stopVideoRecording();

    String videoPath = _videoFile!.path;

    final List<dynamic> images = await platform.invokeMethod('extractFrames', {
      'path': videoPath,
    });

    SharedPreferences prefs = await SharedPreferences.getInstance();

    List<String> base64Images = images.map((img) => base64Encode(img)).toList();
    String jsonBody = jsonEncode(base64Images);

    await http.post(
      //Uri.parse("http://10.0.2.2:5050/users/scan-face"),
      Uri.parse("http://192.168.8.29:5050/users/scan-face"),
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer ${prefs.getString("jwt_token")}",
      },
      body: jsonBody,
    );

    setState(() => loader = false);
  }

  @override
  void dispose() {
    _cameraController?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text("Scan Face")),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            loader
                ? CircularProgressIndicator()
                : _cameraController == null ||
                    !_cameraController!.value.isInitialized
                ? CircularProgressIndicator()
                : SizedBox(
                  height: 600,
                  child: AspectRatio(
                    aspectRatio: 1 / _cameraController!.value.aspectRatio,
                    child: Transform.scale(
                      scaleX: -1,
                      child: CameraPreview(_cameraController!),
                    ),
                  ),
                ),
            SizedBox(height: 20),
            ElevatedButton(
              onPressed: _isRecording ? null : _startRecording,
              child: Text(_isRecording ? counter.toString() : "Start Scanning"),
            ),
          ],
        ),
      ),
    );
  }
}
