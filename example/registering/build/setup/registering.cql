DROP KEYSPACE IF EXISTS registering;
CREATE KEYSPACE registering WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 };

DROP TABLE IF EXISTS registering.registration;
CREATE TABLE registering.registration
(
    email        text PRIMARY KEY,
    id           bigint,
    name         text,
    password     text,
    otp          text,
);
CREATE INDEX ON registering.registration(id);

DROP TABLE IF EXISTS registering.registered;
CREATE TABLE re_registering.registered
(
    email        text PRIMARY KEY
);


