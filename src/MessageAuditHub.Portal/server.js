var http = require('http');
var express = require('express');
var app = express();

var mongo = require("mongodb");

var port = process.env.port || 1337;
var controllers = require("./controllers");

controllers.init(app);


var server = http.createServer(app);
server.listen(port);