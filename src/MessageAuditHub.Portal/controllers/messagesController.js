(function (messagesController) {
    
    var messages = require("../data").messages;
    var elasticsearch = require('elasticsearch');
    
    messagesController.init = function (app) {
        app.get("/messages/getByCorrelationId/:correlationId", function (req, res) {
            var correlationId = decodeURIComponent(req.params.correlationId);
            
            console.log('received request for messages with correlation id ' + correlationId);
            
            messages.getMessagesByCorrelationId(function (err, results) {
                if (!err) {
                    console.log('found ' + results.length + ' with correlationId' + correlationId);
                    res.send(results);
                }
            }, correlationId);
        });
        
        app.get("/messages/fullTextSearch/:text", function (req, res) {
            var correlationId = decodeURIComponent(req.params.text);
            
            var client = new elasticsearch.Client({
                host: 'localhost:9200',
                log: 'trace'
            });
            
            client.search({
                // index: 'messageauditstore',
                body: {
                    query: {
                        match: {
                            DeserializedBody: {
                                "query": "948736",
                                "fuzziness": 1
                            }
                        }
                    },
                    "explain": true
                }
            }).then(function (resp) {
                    var expl = resp.hits._explanation;
                    var hits = resp.hits.hits;
                    res.send(hits);
                }, function (err) {
                    console.trace(err.message);
                });
            res.show(hits);

        });

    };


})(module.exports);