CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE TABLE users (
    "Id" uuid DEFAULT uuid_generate_v4() PRIMARY KEY,
    "Email" VARCHAR(255),
    "Name" VARCHAR(255),
    "Data" jsonb not null default '{}',
    "InsertedAt" TIMESTAMP WITHOUT TIME ZONE DEFAULT (now() at time zone 'utc'),
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE DEFAULT (now() at time zone 'utc')
);
CREATE TRIGGER update_updated_at_user BEFORE
UPDATE ON users FOR EACH ROW EXECUTE PROCEDURE update_updated_at();