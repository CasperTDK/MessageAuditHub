var http = require('http');
var express = require('express');
var fs = require('fs');
var app = express();
var elasticsearch = require('elasticsearch');
var mongo = require("mongodb");

var port = process.env.port || 1337;
var controllers = require("./controllers");

app.use(express.static('./public'));

app.set('view options', { layout: false })
   .set('views', __dirname + '/views')
   .engine('html', function (path, options, cb) {
        fs.readFile(path, 'utf-8', cb);
    });

controllers.init(app);

app.get('/', function (req, res) {
    res.render('index.html');
});

var server = http.createServer(app);
server.listen(port);