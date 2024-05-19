CREATE TABLE "users" (
	"Id" UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	"Name" VARCHAR(255),
	"Email" VARCHAR(255),
	"SubscriptionId" VARCHAR(255),
	FOREIGN KEY ("SubscriptionId") REFERENCES Subscriptions("Id")
);