const sqlite3 = require('./node_modules/sqlite3').verbose();
const path = require('path');

const dbPath = path.join(__dirname, '../src/Presentation/Nop.Web/App_Data/nop.sqlite');

const db = new sqlite3.Database(dbPath, (err) => {
    if (err) console.error(err);
});

db.serialize(() => {
    db.run("UPDATE Customer SET VendorId = 1 WHERE Email = 'admin@yourStore.com'", function(err) {
        if (err) console.error(err);
        else console.log(`Updated admin customer VendorId to 1. Rows affected: ${this.changes}`);
    });
});

db.close();
