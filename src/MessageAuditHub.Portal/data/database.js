(function (database) {
    
    var mongodb = require('mongodb');
    var connectionString = 'mongodb://localhost/auditHub';
    var theDb = null;
    
    database.getDb = function (next) {
        if (!theDb) {
            mongodb.MongoClient.connect(connectionString, function (err, db) {
                if (err) {
                    next(err, null);
                } else {
                    theDb = {
                        db: db,
                        messages: db.collection('messageAudit')
                    };
                    next(null, theDb);
                }
            });
        } else {
            next(null, theDb);
        }
    };
})(module.exports)