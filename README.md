# Flexible Catalog POC

ASP.NET Core **.NET 10** API + Razor Pages using **MongoDB Atlas**. This is a NoSQL demo: phones, sofas, and apparel share one `products` collection, each with different nested `attributes`, embedded variants/reviews, and a cart document that snapshots line items plus a mocked payment.

A relational model would need EAV tables or per-category schemas. That is the point of this POC.

## Prerequisites

- .NET 10 SDK
- A free [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) M0 cluster (MongoDB’s own cloud — no AWS/Azure account)

## Atlas setup (end to end)

You do **not** need an AWS or Azure account. Atlas is MongoDB’s own free cloud. If the cluster wizard asks for a cloud provider, pick any region (AWS/GCP/Azure is only where *MongoDB* hosts the cluster).

### 1. Create a free Atlas account

1. Open [https://www.mongodb.com/cloud/atlas/register](https://www.mongodb.com/cloud/atlas/register)
2. Sign up with email or Google.
3. Accept the terms and create an **Organization** + **Project** (defaults are fine, e.g. project name `FlexibleCatalogPoc`).

### 2. Deploy a free cluster

1. In the project, click **Create** / **Build a Database**.
2. Choose the **Free** tier (M0 / Flex free). One free cluster per project.
3. Cluster name: `Cluster0` is fine.
4. Provider/region: closest to you. Click **Create Deployment**.
5. Wait until the cluster status is **Idle**.

### 3. Create a database user (this is *not* your Atlas login)

During the security quickstart, or later under **Database Access**:

1. Authentication: **Password**.
2. Username: e.g. `catalogpoc`.
3. Password: click **Autogenerate**, then **Copy** it somewhere safe.
4. Privileges: **Atlas admin** or **Read and write to any database**.
5. Click **Create User**.

This username/password go **into the connection string**. Your Atlas website login is separate.

### 4. Allow your PC to connect (Network Access)

Atlas blocks every IP until you allow one.

1. Left menu: **Network Access** → **Add IP Address**.
2. Click **Add Current IP Address** (best), **or** for a local POC only: `0.0.0.0/0` (Allow access from anywhere).
3. Confirm and wait until the entry is **Active**.

If you later get `IP not in whitelist` / `AtlasError 8000`, your home IP changed — add it again.

### 5. Copy the connection string

1. On **Database** (clusters), click **Connect** on your cluster.
2. Choose **Drivers**.
3. Driver: **C# / .NET**, version: latest.
4. Copy the URI. It looks like:

```text
mongodb+srv://catalogpoc:<password>@cluster0.xxxxx.mongodb.net/?appName=Cluster0
```

5. Replace `<password>` with the **database user** password from step 3. Do **not** leave the angle brackets.

The host **must** include Atlas's unique id, for example `cluster0.abc12.mongodb.net`. The sample host `cluster.mongodb.net` is a placeholder and will fail with a DNS timeout (`DnsResponseException` / TXT lookup on port 53).

If the password contains `@ : / # ? %` you must URL-encode it, e.g. `P@ss` → `P%40ss`.

### 6. Paste it into this app

Edit `appsettings.Development.json` (not `appsettings.json` if you can avoid it):

```json
"MongoDb": {
  "ConnectionString": "mongodb+srv://catalogpoc:YOUR_PASSWORD@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority",
  "DatabaseName": "flexible-catalog"
}
```

- Keep `DatabaseName` as `flexible-catalog`. The app creates that database on first run.
- Do not commit a real password to git.

### 7. Run and confirm

```bash
cd FlexibleCatalogPoc
dotnet run --launch-profile http
```

You should **not** see `MongoDB connection string is still a placeholder`. Open http://localhost:5017/ — the catalog should list phones, sofas, and tees. In Atlas → **Browse Collections**, you should see database `flexible-catalog` with `products` and `carts`.

On first run the app creates indexes and seeds seven products if the `products` collection is empty.

## Run

```bash
cd FlexibleCatalogPoc
dotnet run
```

- UI catalog: `https://localhost:7089/`
- Cart: `https://localhost:7089/cart`
- Swagger: `https://localhost:7089/swagger`

HTTP profile (no TLS): `http://localhost:5017`

## Demo script

1. Open Swagger → `GET /api/products`. Compare `attributes` on a phone vs a sofa vs a tee.
2. `GET /api/products/search?storageGb=128` (nested field on electronics) then `?fabric=linen` (nested field on furniture).
3. `POST /api/products` with a furniture-only body (see `FlexibleCatalogPoc.http`) — no schema migration.
4. In the Razor UI, add a phone and a sofa to the same cart. Apply promo `SAVE10`.
5. Click **Checkout**. The overlay shows **Payment processing...**, then the cart document shows **Payment done** with a fake `DEMO-...` transaction id. There is no Stripe/PayPal.

## API

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/api/products?category=` | List; optional category filter |
| GET | `/api/products/{id}` | Product document |
| GET | `/api/products/search` | Text `q` plus nested filters `storageGb`, `ramGb`, `fabric`, `material` |
| POST | `/api/products` | Create with arbitrary `attributes` JSON |
| POST | `/api/products/{id}/reviews` | Append an embedded review |
| GET | `/api/carts/demo` | Get or create shopper `demo-user` |
| POST | `/api/carts/demo/items` | Add a line item (`productId`, optional variant `sku`, `qty`) |
| DELETE | `/api/carts/demo/items/{sku}` | Remove a line item |
| POST | `/api/carts/demo/promo` | Only `SAVE10` is implemented |
| POST | `/api/carts/demo/checkout` | Mock payment: Processing → Paid |

## Out of scope

Real payments, auth, inventory reservations, Docker, AWS/Azure, EF Core/SQL.
