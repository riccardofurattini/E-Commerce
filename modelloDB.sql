-- Crea il database se non esiste
CREATE DATABASE magazzino_db;

-- Seleziona il database
\c magazzino_db;

-- Crea la tabella "items"
CREATE TABLE items (
    Id UUID PRIMARY KEY,  -- Tipo UUID per il campo Id
    Nome VARCHAR(100),    -- Nome dell'articolo
    Descrizione VARCHAR(255),  -- Descrizione dell'articolo
    Prezzo DOUBLE PRECISION,  -- Prezzo dell'articolo
    Quantita INT          -- Quantità disponibile
);

-- Inserisci alcuni dati di esempio nella tabella
INSERT INTO items (Id, Nome, Descrizione, Prezzo, Quantita)
VALUES
    ('d4d1f8b7-8f2f-4c42-bb4d-f0d028cf9b3f', 'Articolo 1', 'Descrizione Articolo 1', 10.99, 100),
    ('cb1a7c1e-559a-4de7-9a42-b388db30a4a1', 'Articolo 2', 'Descrizione Articolo 2', 25.50, 50),
    ('e4c87b13-b19b-4ff2-95b8-d8306f16c0a7', 'Articolo 3', 'Descrizione Articolo 3', 14.75, 75);

-- Verifica che i dati siano stati inseriti
SELECT * FROM items;
