'use strict';
var finalhandler = require('finalhandler');
var http = require('http');
var port = process.env.PORT || 1337;
var serveStatic = require('serve-static');
var serve = serveStatic('./pages', { 'index': ['main-screen.html'] })

http.createServer(function (req, res) {
    serve(req, res, finalhandler(req, res))
}).listen(port);

