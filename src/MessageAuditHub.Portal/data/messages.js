(function (messages) {
    
    var database = require('./database');
    
    messages.getMessagesByCorrelationId = function (next, correlationId) {
        database.getDb(function (err, db) {
            if (err) {
                next(err, null);
            } 
            else {
                db.messages.find({ CorrelationId : correlationId }).toArray(function (err, results) {
                    if (err) {
                        next(err, null);
                    } else {
                        next(null, results);
                    }
                });
            }
        });
    };

})(module.exports)