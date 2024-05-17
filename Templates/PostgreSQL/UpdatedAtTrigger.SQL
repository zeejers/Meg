CREATE FUNCTION update_updated_at() RETURNS TRIGGER AS $$ BEGIN NEW."UpdatedAt" = now();
RETURN NEW;
END;
$$ language 'plpgsql';