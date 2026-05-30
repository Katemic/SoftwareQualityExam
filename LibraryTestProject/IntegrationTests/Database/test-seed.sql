-- test-seed.sql
-- Seed data for integration tests.
-- This file assumes the database has already been reset.

INSERT INTO `language` (`id`, `language`) VALUES
(1, 'English'),
(2, 'Danish'),
(3, 'German'),
(4, 'Swedish'),
(5, 'Norwegian'),
(6, 'French'),
(7, 'Spanish'),
(8, 'Italian'),
(9, 'Dutch'),
(10, 'Polish');

INSERT INTO `publisher` (`id`, `name`) VALUES
(1, 'Penguin Random House'),
(2, 'HarperCollins'),
(3, 'Macmillan Publishers'),
(4, 'Simon & Schuster'),
(5, 'Hachette Livre'),
(6, 'Gyldendal'),
(7, 'Lindhardt og Ringhof'),
(8, 'Asmodee'),
(9, 'Kosmos'),
(10, 'Days of Wonder');

INSERT INTO `genre` (`id`, `name`) VALUES
(1, 'Fantasy'),
(2, 'Science Fiction'),
(3, 'Mystery'),
(4, 'Thriller'),
(5, 'Romance'),
(6, 'Non-fiction'),
(7, 'Strategy'),
(8, 'Family'),
(9, 'Adventure'),
(10, 'History');

INSERT INTO `tag` (`id`, `name`) VALUES
(1, 'Bestseller'),
(2, 'Classic'),
(3, 'Award Winner'),
(4, 'Co-op'),
(5, 'Deckbuilding'),
(6, 'Easy to Learn'),
(7, 'Deep Strategy'),
(8, 'For Kids'),
(9, 'Standalone'),
(10, 'Short Sessions');

INSERT INTO `loaner` (`id`, `first_name`, `last_name`, `cpr`, `tlf`, `email`, `password`) VALUES
(1, 'Mads', 'Jensen', '010190-1234', '12345678', 'mads.jensen@example.com', '$2b$12$0wD2wZJj7lKUCJkFScpF5.PVo9WYllp7hzXSfEtMCk0HjGEAJ6AvC'),
(2, 'Sofie', 'Hansen', '120295-5678', '87654321', 'sofie.hansen@example.com', '$2b$12$0wD2wZJj7lKUCJkFScpF5.PVo9WYllp7hzXSfEtMCk0HjGEAJ6AvC'),
(3, 'Lars', 'Nielsen', '050388-9999', '11223344', 'lars.nielsen@example.com', '$2b$12$0wD2wZJj7lKUCJkFScpF5.PVo9WYllp7hzXSfEtMCk0HjGEAJ6AvC'),
(4, 'Emma', 'Pedersen', '220498-2222', '22334455', 'emma.pedersen@example.com', '$2b$12$0wD2wZJj7lKUCJkFScpF5.PVo9WYllp7hzXSfEtMCk0HjGEAJ6AvC'),
(5, 'Noah', 'Christensen', '300101-3333', '33445566', 'noah.christensen@example.com', '$2b$12$0wD2wZJj7lKUCJkFScpF5.PVo9WYllp7hzXSfEtMCk0HjGEAJ6AvC'),
(11, 'Three', 'Loans', '010101-1111', '11111111', 'three.loans@example.com', '$2b$12$0wD2wZJj7lKUCJkFScpF5.PVo9WYllp7hzXSfEtMCk0HjGEAJ6AvC');

INSERT INTO `item`
(`id`, `name`, `release_year`, `description`, `review_summary`, `media_type`, `image`, `language_id`, `publisher_id`, `average_stars`) VALUES
(1, 'Harry Potter and the Philosopher''s Stone', 1997, 'A young wizard discovers his destiny.', 'Beloved modern fantasy.', 'book', 'hp1.jpg', 1, 1, 4.8),
(11, 'Catan', 1995, 'Trade and build settlements.', 'Family strategy staple.', 'boardgame', 'catan.jpg', 1, 8, 4.5),
(12, 'Pandemic', 2008, 'Work together to stop outbreaks.', 'Top co-op.', 'boardgame', 'pandemic.jpg', 1, 10, 4.6);

INSERT INTO `inventory` (`id`, `item_id`, `status`, `barcode`, `placement`) VALUES
(1, 1, 'loaned out', 'BC0001', 'Shelf A1'),
(2, 1, 'loaned out', 'BC0002', 'Shelf A1'),
(21, 11, 'available', 'BC0021', 'Boardgame 1'),
(23, 12, 'available', 'BC0023', 'Boardgame 2'),
(31, 11, 'loaned out', 'BC0031', 'Boardgame 1'),
(32, 11, 'loaned out', 'BC0032', 'Boardgame 1'),
(33, 11, 'loaned out', 'BC0033', 'Boardgame 1');

INSERT INTO `loan`
(`id`, `loan_date`, `due_date`, `return_date`, `status`, `loaner_id`, `inventory_id`) VALUES
(1, '2026-02-10 10:00:00', '2026-02-24 10:00:00', NULL, 'overdue', 1, 2),
(2, '2026-02-15 12:00:00', '2026-02-28 12:00:00', NULL, 'active', 2, 1),
(21, '2026-03-01 10:00:00', '2026-03-15 10:00:00', NULL, 'active', 11, 31),
(22, '2026-03-02 10:00:00', '2026-03-16 10:00:00', NULL, 'active', 11, 32),
(23, '2026-03-03 10:00:00', '2026-03-17 10:00:00', NULL, 'active', 11, 33);

INSERT INTO `fine`
(`id`, `amount`, `status`, `created_date`, `paid_date`, `loan_id`) VALUES
(1, 25.00, 'unpaid', '2026-02-25 09:00:00', NULL, 1);