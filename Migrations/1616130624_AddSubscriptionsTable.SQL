CREATE TABLE "Subscriptions" (
	"Id" UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	"Name" VARCHAR(255)
);