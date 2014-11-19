(function(controllers) {

    var messagesController = require("./messagesController");
    
    controllers.init = function(app) {
        messagesController.init(app);
    };

})(module.exports)