CREATE TABLE "stadium" (
  "id" SERIAL PRIMARY KEY,
  "name" VARCHAR(255) NOT NULL,
  "country" VARCHAR(255) NOT NULL,
  "address" TEXT NOT NULL,
  "capacity" INT NOT NULL
);

CREATE TABLE "events" (
  "id" SERIAL PRIMARY KEY,
  "type" VARCHAR(255) NOT NULL,
  "capacity" INT NOT NULL,
  "date" TIMESTAMP NOT NULL,
  "stadium_id" INT REFERENCES "stadium" ("id") NOT NULL
);

CREATE TABLE "users" (
  "id" SERIAL PRIMARY KEY,
  "name" VARCHAR(255) NOT NULL,
  "email" VARCHAR(255) NOT NULL,
  "birthdate" DATE NOT NULL,
  "phone_number" VARCHAR(15) NOT NULL
);

CREATE TABLE "reservation" (
  "id" SERIAL PRIMARY KEY,
  "date" TIMESTAMP NOT NULL,
  "event_id" INT REFERENCES "events" ("id") NOT NULL,
  "users_id" INT REFERENCES "users" ("id") NOT NULL
);

CREATE TABLE "seats" (
  "id" SERIAL PRIMARY KEY,
  "code" VARCHAR(100) NOT NULL,
  "event_id" INT REFERENCES "events" ("id") NOT NULL
);

CREATE TABLE "reservations_seats" (
  "id" SERIAL PRIMARY KEY,
  "seat_id" INT REFERENCES "seats" ("id") NOT NULL,
  "revervation_id" INT REFERENCES "reservation" ("id") NOT NULL
);
