(function(messagesController) {

    var messages = require("../data").messages;

    messagesController.init = function(app) {
        app.get("/messages/getByCorrelationId/:correlationId", function (req, res) {
            var correlationId = decodeURIComponent(req.params.correlationId);
            
            console.log('received request for messages with correlation id ' + correlationId);

            messages.getMessagesByCorrelationId(function(err, results) {
                if (!err) {
                    console.log('found ' + results.length + ' with correlationId' + correlationId);
                    res.send(results);
                }
            }, correlationId);
        });

    };


})(module.exports);