-- Add extension for managing timestamps with triggers (if not already installed)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

SET TIME ZONE 'Asia/Ho_Chi_Minh'

CREATE SEQUENCE IF NOT EXISTS Expense_Id_seq;
CREATE SEQUENCE IF NOT EXISTS Category_Id_seq;
CREATE SEQUENCE IF NOT EXISTS Account_Id_seq;
CREATE SEQUENCE IF NOT EXISTS Income_Id_seq;
CREATE SEQUENCE IF NOT EXISTS Transfer_Id_seq;
CREATE SEQUENCE IF NOT EXISTS Budget_Id_seq;
CREATE SEQUENCE IF NOT EXISTS User_Id_seq;
CREATE SEQUENCE IF NOT EXISTS Currency_Id_seq;

CREATE TABLE IF NOT EXISTS "Expense" (
  Id integer NOT NULL DEFAULT nextval('Expense_Id_seq') PRIMARY KEY,
  Title varchar(50),
  Description varchar(255),
  Amount decimal NOT NULL,
  CreatedAt timestamp NOT NULL DEFAULT NOW(),
  UpdatedAt timestamp NOT NULL DEFAULT NOW(),
  AccountId integer NOT NULL,
  CategoryId integer
);

CREATE TABLE IF NOT EXISTS "Category" (
  Id integer NOT NULL DEFAULT nextval('Category_Id_seq') PRIMARY KEY,
  Name varchar(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS "Account" (
  Id integer NOT NULL DEFAULT nextval('Account_Id_seq') PRIMARY KEY,
  Name varchar(50) NOT NULL,
  Balance decimal NOT NULL DEFAULT 0,
  CreatedAt timestamp NOT NULL DEFAULT NOW(),
  UpdatedAt timestamp NOT NULL DEFAULT NOW(),
  UserId integer NOT NULL
);

CREATE TABLE IF NOT EXISTS "Income" (
  Id integer NOT NULL DEFAULT nextval('Income_Id_seq') PRIMARY KEY,
  Title varchar(50),
  Description varchar(255),
  Amount decimal NOT NULL,
  CreatedAt timestamp NOT NULL DEFAULT NOW(),
  UpdatedAt timestamp NOT NULL DEFAULT NOW(),
  AccountId integer NOT NULL,
  CategoryId integer
);

CREATE TABLE IF NOT EXISTS "Transfer" (
  Id integer NOT NULL DEFAULT nextval('Transfer_Id_seq') PRIMARY KEY,
  Title varchar(50),
  Description varchar(255),
  Amount decimal NOT NULL,
  CreatedAt timestamp NOT NULL DEFAULT NOW(),
  UpdatedAt timestamp NOT NULL DEFAULT NOW(),
  SourceAccountId integer NOT NULL,
  TargetAccountId integer NOT NULL,
  CategoryId integer
);

CREATE TABLE IF NOT EXISTS "Budget" (
  Id integer NOT NULL DEFAULT nextval('Budget_Id_seq') PRIMARY KEY,
  Name varchar(50) NOT NULL,
  StartDate date NOT NULL,
  EndDate date NOT NULL,
  Period integer NOT NULL,
  Current decimal NOT NULL DEFAULT 0,
  BudgetLimit decimal NOT NULL,
  CreatedAt timestamp NOT NULL DEFAULT NOW(),
  UpdatedAt timestamp NOT NULL DEFAULT NOW(),
  UserId integer NOT NULL,
  CategoryId integer
);

CREATE TABLE IF NOT EXISTS "User" (
  Id integer NOT NULL DEFAULT nextval('User_Id_seq') PRIMARY KEY,
  Name varchar(255) NOT NULL,
  Email varchar(255) NOT NULL,
  CreatedAt timestamp NOT NULL DEFAULT NOW(),
  UpdatedAt timestamp NOT NULL DEFAULT NOW(),
  CurrencyId integer NOT NULL
);

CREATE TABLE IF NOT EXISTS "Currency" (
  Id integer NOT NULL DEFAULT nextval('Currency_Id_seq') PRIMARY KEY,
  Name varchar(50) NOT NULL,
  Code varchar(10) NOT NULL,
  Sign varchar(10) NOT NULL
);

ALTER TABLE "User" ADD CONSTRAINT User_CurrencyId_fk FOREIGN KEY (CurrencyId) REFERENCES "Currency" (Id);
ALTER TABLE "Account" ADD CONSTRAINT Account_UserId_fk FOREIGN KEY (UserId) REFERENCES "User" (Id);
ALTER TABLE "Transfer" ADD CONSTRAINT Transfer_SourceAccountId_fk FOREIGN KEY (SourceAccountId) REFERENCES "Account" (Id) ON DELETE CASCADE;
ALTER TABLE "Transfer" ADD CONSTRAINT Transfer_TargetAccountId_fk FOREIGN KEY (TargetAccountId) REFERENCES "Account" (Id) ON DELETE CASCADE;
ALTER TABLE "Transfer" ADD CONSTRAINT Transfer_CategoryId_fk FOREIGN KEY (CategoryId) REFERENCES "Category" (Id);
ALTER TABLE "Budget" ADD CONSTRAINT Budget_UserId_fk FOREIGN KEY (UserId) REFERENCES "User" (Id);
ALTER TABLE "Budget" ADD CONSTRAINT Budget_CategoryId_fk FOREIGN KEY (CategoryId) REFERENCES "Category" (Id);
ALTER TABLE "Income" ADD CONSTRAINT Income_AccountId_fk FOREIGN KEY (AccountId) REFERENCES "Account" (Id) ON DELETE CASCADE;
ALTER TABLE "Income" ADD CONSTRAINT Income_CategoryId_fk FOREIGN KEY (CategoryId) REFERENCES "Category" (Id);
ALTER TABLE "Expense" ADD CONSTRAINT Expense_AccountId_fk FOREIGN KEY (AccountId) REFERENCES "Account" (Id) ON DELETE CASCADE;
ALTER TABLE "Expense" ADD CONSTRAINT Expense_CategoryId_fk FOREIGN KEY (CategoryId) REFERENCES "Category" (Id);

CREATE OR REPLACE FUNCTION update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
   NEW."updatedat" = CURRENT_TIMESTAMP;
   RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_budget_on_expense_insert()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Budget"
    SET "current" = "current" + NEW."amount"
    WHERE "enddate" >= NEW."date"
      AND "startdate" <= NEW."date"
      AND ("categoryid" IS NULL OR "categoryid" = NEW."categoryid")
      AND "userid" = (
          SELECT "userid"
          FROM "Account"
          WHERE "id" = NEW."accountid"
      );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_budget_on_expense_delete()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Budget"
    SET "current" = "current" - OLD."amount"
    WHERE "enddate" >= OLD."date"
      AND "startdate" <= OLD."date"
      AND ("categoryid" IS NULL OR "categoryid" = OLD."categoryid")
      AND "userid" = (
          SELECT "userid"
          FROM "Account"
          WHERE "id" = OLD."accountid"
      );

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_budget_on_expense_update()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Budget"
    SET "current" = "current" - OLD."amount"
    WHERE "enddate" >= OLD."date"
      AND "startdate" <= OLD."date"
      AND ("categoryid" IS NULL OR "categoryid" = OLD."categoryid")
      AND "userid" = (
          SELECT "userid"
          FROM "Account"
          WHERE "id" = OLD."accountid"
      );

    UPDATE "Budget"
    SET "current" = "current" + NEW."amount"
    WHERE "enddate" >= NEW."date"
      AND "startdate" <= NEW."date"
      AND ("categoryid" IS NULL OR "categoryid" = NEW."categoryid")
      AND "userid" = (
          SELECT "userid"
          FROM "Account"
          WHERE "id" = NEW."accountid"
      );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_account_on_income_insert()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Account"
    SET "balance" = "balance" + NEW."amount"
    WHERE "id" = NEW."accountid";

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_account_on_income_update()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW."accountid" != OLD."accountid" THEN
        UPDATE "Account"
        SET "balance" = "balance" - OLD."amount"
        WHERE "id" = OLD."AccountId";

        UPDATE "Account"
        SET "balance" = "balance" + NEW."amount"
        WHERE "id" = NEW."accountid";
    ELSE
        UPDATE "Account"
        SET "balance" = "balance" + (NEW."amount" - OLD."amount")
        WHERE "id" = NEW."accountid";
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_account_on_income_delete()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Account"
    SET "balance" = "balance" - OLD."amount"
    WHERE "id" = OLD."accountid";

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_account_on_expense_insert()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Account"
    SET "balance" = "balance" - NEW."amount"
    WHERE "id" = NEW."accountid";

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_account_on_expense_update()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW."accountid" != OLD."accountid" THEN
        UPDATE "Account"
        SET "balance" = "balance" + OLD."amount"
        WHERE "id" = OLD."accountid";

        UPDATE "Account"
        SET "balance" = "balance" - NEW."amount"
        WHERE "id" = NEW."accountid";
    ELSE
        UPDATE "Account"
        SET "balance" = "balance" - (NEW."amount" - OLD."amount")
        WHERE "id" = NEW."accountid";
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION update_account_on_expense_delete()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Account"
    SET "balance" = "balance" + OLD."amount"
    WHERE "id" = OLD."accountid";

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_accounts_on_transfer_insert()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Account"
    SET "balance" = "balance" - NEW."amount"
    WHERE "id" = NEW."sourceaccountid";

    UPDATE "Account"
    SET "balance" = "balance" + NEW."amount"
    WHERE "id" = NEW."targetaccountid";

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_accounts_on_transfer_update()
RETURNS TRIGGER AS $$
BEGIN
    -- Case 1: SourceAccountId and TargetAccountId both change
    IF NEW."sourceaccountid" != OLD."sourceaccountid" AND NEW."targetaccountid" != OLD."targetaccountid" THEN
        -- Restore the old source account
        UPDATE "Account"
        SET "balance" = "balance" + OLD."amount"
        WHERE "id" = OLD."sourceaccountid";

        -- Reverse the old target account
        UPDATE "Account"
        SET "balance" = "balance" - OLD."amount"
        WHERE "id" = OLD."targetaccountid";

        -- Subtract from the new source account
        UPDATE "Account"
        SET "balance" = "balance" - NEW."amount"
        WHERE "id" = NEW."sourceaccountid";

        -- Add to the new target account
        UPDATE "Account"
        SET "balance" = "balance" + NEW."amount"
        WHERE "id" = NEW."targetaccountid";

    -- Case 2: Only SourceAccountId changes
    ELSIF NEW."sourceaccountid" != OLD."sourceaccountid" THEN
        -- Restore the old source account
        UPDATE "Account"
        SET "balance" = "balance" + OLD."amount"
        WHERE "id" = OLD."sourceaccountid";

        -- Subtract from the new source account
        UPDATE "Account"
        SET "balance" = "balance" - NEW."amount"
        WHERE "id" = NEW."sourceaccountid";

    -- Case 3: Only TargetAccountId changes
    ELSIF NEW."targetaccountid" != OLD."targetaccountid" THEN
        -- Reverse the old target account
        UPDATE "Account"
        SET "balance" = "balance" - OLD."amount"
        WHERE "id" = OLD."targetaccountid";

        -- Add to the new target account
        UPDATE "Account"
        SET "balance" = "balance" + NEW."amount"
        WHERE "id" = NEW."targetaccountid";

    -- Case 4: Only Amount changes
    ELSIF NEW."amount" != OLD."amount" THEN
        -- Adjust the source account
        UPDATE "Account"
        SET "balance" = "balance" + (OLD."amount" - NEW."amount")
        WHERE "id" = OLD."sourceaccountid";

        -- Adjust the target account
        UPDATE "Account"
        SET "balance" = "balance" + (NEW."amount" - OLD."amount")
        WHERE "id" = OLD."targetaccountid";
    END IF;

    -- Return the updated row
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION update_accounts_on_transfer_delete()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Account"
    SET "balance" = "balance" + OLD."amount"
    WHERE "id" = OLD."sourceaccountid";

    UPDATE "Account"
    SET "balance" = "balance" - OLD."amount"
    WHERE "id" = OLD."targetaccountid";

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON "User"
FOR EACH ROW
EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON "Account"
FOR EACH ROW
EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON "Income"
FOR EACH ROW
EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON "Expense"
FOR EACH ROW
EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON "Budget"
FOR EACH ROW
EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER set_updated_at
BEFORE UPDATE ON "Transfer"
FOR EACH ROW
EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER income_insert_trigger
AFTER INSERT ON "Income"
FOR EACH ROW
EXECUTE FUNCTION update_account_on_income_insert();

CREATE TRIGGER income_update_trigger
AFTER UPDATE ON "Income"
FOR EACH ROW
EXECUTE FUNCTION update_account_on_income_update();

CREATE TRIGGER income_delete_trigger
AFTER DELETE ON "Income"
FOR EACH ROW
EXECUTE FUNCTION update_account_on_income_delete();

CREATE TRIGGER expense_insert_trigger
AFTER INSERT ON "Expense"
FOR EACH ROW
EXECUTE FUNCTION update_account_on_expense_insert();

CREATE TRIGGER expense_update_trigger
AFTER UPDATE ON "Expense"
FOR EACH ROW
EXECUTE FUNCTION update_account_on_expense_update();

CREATE TRIGGER expense_delete_trigger
AFTER DELETE ON "Expense"
FOR EACH ROW
EXECUTE FUNCTION update_account_on_expense_delete();

CREATE TRIGGER transfer_insert_trigger
AFTER INSERT ON "Transfer"
FOR EACH ROW
EXECUTE FUNCTION update_accounts_on_transfer_insert();

CREATE TRIGGER transfer_update_trigger
AFTER UPDATE ON "Transfer"
FOR EACH ROW
EXECUTE FUNCTION update_accounts_on_transfer_update();

CREATE TRIGGER transfer_delete_trigger
AFTER DELETE ON "Transfer"
FOR EACH ROW
EXECUTE FUNCTION update_accounts_on_transfer_delete();

CREATE TRIGGER expense_insert_trigger_budget
AFTER INSERT ON "Expense"
FOR EACH ROW
EXECUTE FUNCTION update_budget_on_expense_insert();

CREATE TRIGGER expense_delete_trigger_budget
AFTER DELETE ON "Expense"
FOR EACH ROW
EXECUTE FUNCTION update_budget_on_expense_delete();

CREATE TRIGGER expense_update_trigger_budget
AFTER UPDATE ON "Expense"
FOR EACH ROW
EXECUTE FUNCTION update_budget_on_expense_update();