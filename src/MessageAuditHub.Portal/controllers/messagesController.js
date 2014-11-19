(function(messagesController) {

    var messages = require("../data").messages;

    messagesController.init = function(app) {
        app.get("/messages/getByCorrelationId/:correlationId", function (req, res) {

            messages.getMessagesByCorrelationId(function(err, results) {
                if (!err) {
                    res.send(results);
                }
            }, req.params.correlationId);
        });

    };


})(module.exports);