CREATE TABLE test.autocad (
    id SERIAL PRIMARY KEY,
    id_layer BIGINT NOT NULL,
    name_layer TEXT NOT NULL,
    color_layer TEXT,
    type_layer TEXT NOT NULL,
    geometry test.geometry(geometry, 2236) NOT NULL
);