-- magazzino_db
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
    ('d4d1f8b7-8f2f-4c42-bb4d-f0d028cf9b3f', 'Mele', 'Frutta', 0.99, 500),
    ('cb1a7c1e-559a-4de7-9a42-b388db30a4a1', 'Pere', 'Frutta', 1.50, 400),
    ('e4c87b13-b19b-4ff2-95b8-d8306f16c0a7', 'Banane', 'Frutta', 2.50, 700);


-- store_db
CREATE TABLE articoli (
    Id UUID PRIMARY KEY,  
    Nome VARCHAR(100) NOT NULL,    
    Descrizione VARCHAR(255),  
    Prezzo NUMERIC(10, 2) NOT NULL  -- Precisione per rappresentare importi monetari
);

CREATE TABLE carrello (
    IdUtente UUID NOT NULL,
    IdCarrello UUID PRIMARY KEY,
    IdArticolo UUID NOT NULL REFERENCES articoli(Id),   
    Quantita INT NOT NULL
);
