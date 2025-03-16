# Smart Home

## Tech stack:

- API Application - C#, .NET 9.0
- Web Application - HTML, CSS, TypeScript, React
- Mobile Application - Dart, Flutter

The application allows users to fully manage their accounts and properties. Each user can create an account, log in, log
out, edit their profile, reset their password, and delete their account. Deleting an account also removes all buildings
for which the user is the sole owner. An automated emailing system has been implemented to handle account activation,
password resets, and inviting other users to buildings.

The system enables the creation, editing, and deletion of buildings, rooms, and devices. A special type of device is the
camera, which allows video streaming via Web Socket. Streaming occurs between the mobile application and the web, with
the .NET application acting as an intermediary. To start a transmission, the user must download the app, log in, select
a building, a room, and a camera ID. The video stream is displayed to all building owners associated with the camera.

The REST API application was developed in .NET according to industry standards. Exception handling, clear error
messages, and proper response formatting have been carefully implemented. The database is fully secured against unwanted
data entry, and authentication and authorization mechanisms protect access to resources on both the backend and
frontend.

The frontend was built using React, with a primary focus on technical aspects and security. All form fields are
validated, and errors are displayed in a clear and readable format.

Although the mobile application, which turns a smartphone into a camera, offers only basic functionality, it has also
been thoroughly secured against potential abuses.

Web application is hosted on my own server and domain, that also belongs to me, at 260824.xyz/smart-home.
