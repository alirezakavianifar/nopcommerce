const sqlite3 = require('./node_modules/sqlite3').verbose();
const path = require('path');

const dbPath = path.join(__dirname, '../src/Presentation/Nop.Web/App_Data/nop.sqlite');
console.log('Database path:', dbPath);

const db = new sqlite3.Database(dbPath, (err) => {
    if (err) console.error(err);
});

db.serialize(() => {
    db.all("SELECT Id, Name FROM Vendor", (err, rows) => {
        if (err) console.error('Vendor error:', err);
        else console.log('Vendors:', rows);
    });

    db.all("SELECT Id, Email, VendorId FROM Customer WHERE Email = 'admin@yourStore.com'", (err, rows) => {
        if (err) console.error('Customer error:', err);
        else console.log('Admin customer:', rows);
    });
});

db.close();
