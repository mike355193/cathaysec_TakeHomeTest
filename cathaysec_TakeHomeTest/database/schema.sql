-- Reference schema for replacing the required InMemory repositories with a relational database.
CREATE TABLE stocks (
    symbol       varchar(10)   NOT NULL PRIMARY KEY,
    name         nvarchar(100) NOT NULL,
    exchange     varchar(10)   NOT NULL,
    is_active    bit           NOT NULL DEFAULT 1,
    created_at   datetime2(3)  NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at   datetime2(3)  NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE INDEX ix_stocks_name ON stocks(name);

CREATE TABLE orders (
    id           uniqueidentifier NOT NULL PRIMARY KEY,
    symbol       varchar(10)      NOT NULL,
    side         varchar(4)       NOT NULL,
    price        decimal(19, 4)   NOT NULL,
    quantity     int              NOT NULL,
    status       varchar(20)      NOT NULL,
    created_at   datetime2(3)     NOT NULL,
    updated_at   datetime2(3)     NOT NULL,
    row_version  rowversion       NOT NULL,
    CONSTRAINT fk_orders_stocks FOREIGN KEY (symbol) REFERENCES stocks(symbol),
    CONSTRAINT ck_orders_side CHECK (side IN ('Buy', 'Sell')),
    CONSTRAINT ck_orders_price CHECK (price > 0),
    CONSTRAINT ck_orders_quantity CHECK (quantity > 0),
    CONSTRAINT ck_orders_status CHECK (status IN ('Pending', 'Filled', 'Cancelled', 'Rejected'))
);

CREATE INDEX ix_orders_symbol_created_at ON orders(symbol, created_at DESC);
CREATE INDEX ix_orders_status_created_at ON orders(status, created_at DESC);
