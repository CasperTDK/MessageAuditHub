(function(messagesController) {

    var messages = require("../data").messages;

    messagesController.init = function(app) {
        app.get("/messages/getByCorrelationId/:correlationId", function (req, res) {
            var correlationId = req.params.correlationId.replace('å', '#');
            
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