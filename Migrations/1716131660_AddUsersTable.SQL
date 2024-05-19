CREATE TABLE "users" (
	"Id" UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	"Name" VARCHAR(255),
	"Email" VARCHAR(255),
	"SubscriptionId" UUID,
	FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions"("Id")
);