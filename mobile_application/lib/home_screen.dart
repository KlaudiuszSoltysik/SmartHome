import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class Homescreen extends StatefulWidget {
  const Homescreen({super.key});

  @override
  State<Homescreen> createState() => _HomescreenState();
}

class _HomescreenState extends State<Homescreen> {
  Future<void> _logout(BuildContext context) async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    await prefs.remove("jwt_token");
    if (mounted) {
      Navigator.pushReplacementNamed(context, "/");
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.spaceAround,
          children: [
            ElevatedButton(
              onPressed: () => Navigator.pushNamed(context, "/camera"),
              child: Text("Camera"),
            ),
            ElevatedButton(
              onPressed: () => Navigator.pushNamed(context, "/scan"),
              child: Text("Scan Face"),
            ),
            ElevatedButton(
              onPressed: () => _logout(context),
              child: Text("Logout"),
            ),
          ],
        ),
      ),
    );
  }
}
