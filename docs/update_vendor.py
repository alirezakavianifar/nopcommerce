import sqlite3
import os

db_path = r'E:\projects\nopCommerce_4.90.3_Source\src\Presentation\Nop.Web\App_Data\nop.sqlite'
print("DB Path exists:", os.path.exists(db_path))

conn = sqlite3.connect(db_path)
cur = conn.cursor()
cur.execute("SELECT name FROM sqlite_master WHERE type='table';")
tables = [r[0] for r in cur.fetchall()]
print("Tables:", tables)

if "Customer" in tables:
    cur.execute("UPDATE Customer SET VendorId = 1 WHERE Email = 'admin@yourStore.com'")
    conn.commit()
    print("Updated rows:", cur.rowcount)
    cur.execute("SELECT Id, Email, VendorId FROM Customer WHERE Email = 'admin@yourStore.com'")
    print("Admin Customer:", cur.fetchall())

conn.close()
