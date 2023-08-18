CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

--Table
CREATE TABLE IF NOT EXISTS portfolios (
  portfolio_id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  portfolio_uuid UUID DEFAULT uuid_generate_v4(),
  description text NOT NULL,
  created_on timestamp DEFAULT CURRENT_TIMESTAMP
);

--Table
CREATE TABLE IF NOT EXISTS products (
  product_id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  portfolio_id INTEGER REFERENCES portfolios (portfolio_id) NOT NULL,
  product_uuid UUID DEFAULT uuid_generate_v4(),
  payload JSON NOT NULL,
  created_on TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--index
CREATE INDEX IF NOT EXISTS idx_portfolio_uuid ON portfolios (portfolio_id);
CREATE INDEX IF NOT EXISTS idx_product_uuid ON products (product_uuid);
