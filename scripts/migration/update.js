var executeMigrations = function (migrations) {
    while (true) {
        var v = db.meta.findOne({ _id: "databaseVersion" }) || { _id: "databaseVersion", version: 0 };
        var currentVersion = v.version;
        var migration = migrations["" + currentVersion];

        if (typeof migration == "undefined") {
            print("Could not find migration with version " + currentVersion + " - quitting!");
            break;
        }

        print("Executing migration: " + currentVersion);

        try {
            migration();
        }
        catch (exception) {
            print("An error occurred while executing migration " + currentVersion + ": " + exception + " - quitting!");
            break;
        }

        var newVersion = v.version + 1;

        print("Updating database version to " + newVersion);

        v.version = newVersion;
        db.meta.save(v);
    }
};

var migrations = {
    "0": function() {
        print("Added AuditCopyTime");
        db.messageAudit.find({ "AuditCopyTime": { $exists: false } }).forEach(
            function(elem) {
                var message = db.messageAudit.findOne({ "_id": elem._id });

                db.messageAudit.update({ "_id": elem._id }, { $set: { "AuditCopyTime": ISODate(message.Headers["rebus-audit-copy-time"]) } }, false, true);
            });
    },
};

executeMigrations(migrations);