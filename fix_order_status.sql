-- Convert OrderStatus column to string type
ALTER TABLE Orders MODIFY COLUMN Status VARCHAR(20) NOT NULL;

-- Update existing values to strings
UPDATE Orders SET Status = 'Pending' WHERE Status = '0';
UPDATE Orders SET Status = 'Processing' WHERE Status = '1';
UPDATE Orders SET Status = 'Shipped' WHERE Status = '2';
UPDATE Orders SET Status = 'Delivered' WHERE Status = '3';
UPDATE Orders SET Status = 'Cancelled' WHERE Status = '4';
UPDATE Orders SET Status = 'Completed' WHERE Status = '5';

-- Add the missing PaymentId column to the Orders table
ALTER TABLE Orders ADD COLUMN PaymentId VARCHAR(255) NULL;

-- Update the column to NOT NULL after adding default values for existing records
UPDATE Orders SET PaymentId = 'legacy' WHERE PaymentId IS NULL;
ALTER TABLE Orders MODIFY COLUMN PaymentId VARCHAR(255) NOT NULL;
