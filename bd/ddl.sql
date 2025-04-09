

DROP TABLE IF EXISTS reservation_seats;
DROP TABLE IF EXISTS seat;
DROP TABLE IF EXISTS reservation;
DROP TABLE IF EXISTS "user";
DROP TABLE IF EXISTS "event";
DROP TABLE IF EXISTS stadium;






CREATE TABLE "stadium" (
  "id" SERIAL PRIMARY KEY,
  "name" VARCHAR(255) NOT NULL,
  "country" VARCHAR(255) NOT NULL,
  "address" TEXT NOT NULL,
  "capacity" INT NOT NULL
);

CREATE TABLE "event" (
  "id" SERIAL PRIMARY KEY,
  "type" VARCHAR(255) NOT NULL,
  "capacity" INT NOT NULL,
  "date" TIMESTAMP NOT NULL,
  "stadium_id" INT REFERENCES "stadium" ("id") NOT NULL
);

CREATE TABLE "user" (
  "id" SERIAL PRIMARY KEY,
  "name" VARCHAR(255) NOT NULL,
  "email" VARCHAR(255) NOT NULL,
  "birthdate" DATE NOT NULL,
  "phone_number" VARCHAR(15) NOT NULL
);

CREATE TABLE "reservation" (
  "id" SERIAL PRIMARY KEY,
  "date" TIMESTAMP NOT NULL DEFAULT now(),
  "event_id" INT REFERENCES "event" ("id") NOT NULL,
  "user_id" INT REFERENCES "user" ("id") NOT NULL
);

CREATE TABLE "seat" (
  "id" SERIAL PRIMARY KEY,
  "code" VARCHAR(100) NOT NULL,
  "event_id" INT REFERENCES "event" ("id") NOT NULL,
  "is_available" BOOLEAN DEFAULT true
);

CREATE TABLE "reservation_seats" (
  "id" SERIAL PRIMARY KEY,
  "seat_id" INT REFERENCES "seat" ("id") NOT NULL,
  "reservation_id" INT REFERENCES "reservation" NOT NULL
);
