CREATE DATABASE  IF NOT EXISTS `vkr` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `vkr`;
-- MySQL dump 10.13  Distrib 8.0.41, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: vkr
-- ------------------------------------------------------
-- Server version	8.0.30

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Category`
--

DROP TABLE IF EXISTS `Category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Category` (
  `idCategory` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  PRIMARY KEY (`idCategory`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Category`
--

LOCK TABLES `Category` WRITE;
/*!40000 ALTER TABLE `Category` DISABLE KEYS */;
INSERT INTO `Category` VALUES (1,'Консультации'),(2,'Диагностика'),(3,'Лечение зубов'),(4,'Хирургия'),(5,'Ортопедия'),(6,'Ортодонтия'),(7,'Гигиена'),(8,'Детская стоматология');
/*!40000 ALTER TABLE `Category` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Doctors`
--

DROP TABLE IF EXISTS `Doctors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Doctors` (
  `idDoctors` int NOT NULL AUTO_INCREMENT,
  `Surname` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Name` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Lastname` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Phone_number` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Photo` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `Speciality` int NOT NULL,
  PRIMARY KEY (`idDoctors`),
  KEY `fk_Doctors_Speciality1_idx` (`Speciality`),
  CONSTRAINT `fk_Doctors_Speciality1` FOREIGN KEY (`Speciality`) REFERENCES `Speciality` (`idSpeciality`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=58 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Doctors`
--

LOCK TABLES `Doctors` WRITE;
/*!40000 ALTER TABLE `Doctors` DISABLE KEYS */;
INSERT INTO `Doctors` VALUES (1,'Смирнов','Андрей','Иванович','+7 999 000 00 01','doc2.jpeg',1),(2,'Кузнецова','Ольга','Павловна','+7 999 000 00 02','doc1.jpeg',1),(3,'Васильев','Дмитрий','Алексеевич','+7 999 000 00 03','doc4.jpeg',2),(4,'Михайлова','Елена','Сергеевна','+7 999 000 00 04',NULL,2),(5,'Попов','Николай','Федорович','+7 999 000 00 05','doc5.jpg',3),(6,'Волкова','Ирина','Андреевна','+7 999 000 00 06','doc6.jpg',3),(7,'Соколов','Павел','Николаевич','+7 999 000 00 07','doc7.jpg',4),(8,'Зайцева','Мария','Олеговна','+7 999 000 00 08','doc8.jpg',4),(9,'Павлов','Алексей','Сергеевич','+7 999 000 00 09','doc9.jpg',5),(10,'Борисова','Елена','Николаевна','+7 999 000 00 10','doc10.jpg',5),(11,'Орлов','Сергей','Анатольевич','+7 999 000 00 11','doc11.jpg',6),(12,'Гаврилова','Ирина','Петровна','+7 999 000 00 12','doc12.jpg',6),(14,'Крылова','Анна','Алексеевна','+7 999 000 00 14','doc3.jpeg',1),(15,'Медведев','Игорь','Павлович','+7 999 000 00 15',NULL,2),(22,'Николаев','Владимир','Андреевич','+7 999 000 00 16','doc16.jpg',3),(23,'Федорова','Светлана','Игоревна','+7 999 000 00 17','doc17.jpg',4),(24,'Егоров','Александр','Павлович','+7 999 000 00 18','doc18.jpg',5),(25,'Алексеева','Марина','Сергеевна','+7 999 000 00 19','doc19.jpg',6),(27,'Соловьёв','Максим','Иванович','+7 999 000 00 21','6fgnff.png',1),(28,'Козлова','Татьяна','Александровна','+7 999 000 00 22','doc22.jpg',2),(29,'Беляев','Никита','Сергеевич','+7 999 000 00 23','doc23.jpg',3),(30,'Ковалева','Анна','Фёдоровна','+7 999 000 00 24','doc24.jpg',4),(31,'Зайцев','Роман','Алексеевич','+7 999 000 00 25','doc25.jpg',5),(32,'Панкова','Екатерина','Игоревна','+7 999 000 00 26','doc26.jpg',6),(34,'Семенова','Людмила','Владимировна','+7 999 000 00 28','doc28.jpg',1),(35,'Тихонов','Константин','Анатольевич','+7 999 000 00 29','doc29.jpg',2),(36,'Малиновская','Ольга','Павловна','+7 999 000 00 30','doc30.jpg',3),(37,'Лебедев','Вячеслав','Игоревич','+7 999 000 00 31','doc31.jpg',4),(38,'Киселева','Наталья','Дмитриевна','+7 999 000 00 32','doc32.jpg',5),(39,'Воробьёв','Даниил','Александрович','+7 999 000 00 33','doc33.jpg',6),(41,'Щербаков','Филипп','Иванович','+7 999 000 00 35','doc35.jpg',1),(42,'Куликова','Маргарита','Алексеевна','+7 999 000 00 36','doc36.jpg',2),(43,'Данилов','Артур','Павлович','+7 999 000 00 37','doc37.jpg',3),(44,'Петухова','София','Игоревна','+7 999 000 00 38','doc38.jpg',4),(45,'Сафонов','Евгений','Дмитриевич','+7 999 000 00 39','doc39.jpg',5),(46,'Анисимова','Ксения','Сергеевна','+7 999 000 00 40','doc40.jpg',6),(48,'Белова','Полина','Владимировна','+7 999 000 00 42','doc42.jpg',1),(49,'Кондратьев','Глеб','Николаевич','+7 999 000 00 43','doc43.jpg',2),(50,'Тарасова','Дарья','Александровна','+7 999 000 00 44','doc44.jpg',3),(51,'Мартынов','Павел','Фёдорович','+7 999 000 00 45','doc45.jpg',4),(52,'Ефимова','Юлия','Игоревна','+7 999 000 00 46','doc46.jpg',5),(53,'Лазарев','Владислав','Анатольевич','+7 999 000 00 47','doc47.jpg',6),(55,'Васильев','Семен','Александрович','+7 999 000 00 49','doc49.jpg',1),(56,'Мухина','Елена','Павловна','+7 999 000 00 50','doc50.jpg',2);
/*!40000 ALTER TABLE `Doctors` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Order`
--

DROP TABLE IF EXISTS `Order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Order` (
  `idOrder` int NOT NULL AUTO_INCREMENT,
  `Sum` decimal(10,2) NOT NULL,
  `Schedule` int NOT NULL,
  `User` int NOT NULL,
  `Patients_idPatients` int NOT NULL,
  `Status` int NOT NULL,
  `Discount` decimal(10,2) DEFAULT '0.00',
  `TotalSum` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`idOrder`,`Schedule`,`User`,`Patients_idPatients`,`Status`),
  KEY `fk_Order_Status1_idx` (`Status`),
  KEY `fk_Order_Users1_idx` (`User`),
  KEY `fk_Order_Patients1_idx` (`Patients_idPatients`),
  KEY `fk_Order_Schedule1_idx` (`Schedule`),
  CONSTRAINT `fk_Order_Patients1` FOREIGN KEY (`Patients_idPatients`) REFERENCES `Patients` (`idPatients`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_Order_Schedule1` FOREIGN KEY (`Schedule`) REFERENCES `Schedule` (`idSchedule`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_Order_Status` FOREIGN KEY (`Status`) REFERENCES `StatusesPriem` (`idStatusesPriem`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_Order_Users1` FOREIGN KEY (`User`) REFERENCES `Users` (`idUsers`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=42 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Order`
--

LOCK TABLES `Order` WRITE;
/*!40000 ALTER TABLE `Order` DISABLE KEYS */;
INSERT INTO `Order` VALUES (6,2000.00,8,5,8,2,100.00,1900.00),(7,3000.00,3,5,1,1,150.00,2850.00),(8,18000.00,8,5,7,1,900.00,17100.00),(10,5500.00,10,5,10,1,275.00,5225.00),(11,40000.00,1,5,8,2,2000.00,38000.00),(12,3300.00,6,5,50,3,165.00,3135.00),(13,6200.00,4,5,1,3,310.00,5890.00),(14,7500.00,7,5,1,3,375.00,7125.00),(15,7500.00,12,5,1,3,375.00,7125.00),(16,5000.00,14,5,1,3,250.00,4750.00),(17,7500.00,16,5,1,1,375.00,7125.00),(18,5000.00,18,5,1,3,250.00,4750.00),(19,4000.00,20,5,18,1,200.00,3800.00),(20,0.00,22,5,5,2,0.00,0.00),(22,0.00,24,5,1,2,0.00,0.00),(23,500.00,56,5,8,1,0.00,500.00),(24,0.00,50,5,9,2,0.00,0.00),(25,0.00,26,5,1,3,0.00,0.00),(26,0.00,28,5,7,3,0.00,0.00),(27,0.00,58,5,34,3,0.00,0.00),(28,0.00,30,5,18,3,0.00,0.00),(29,0.00,32,5,10,3,0.00,0.00),(30,0.00,52,5,19,3,0.00,0.00),(31,800.00,34,5,16,3,0.00,800.00),(32,0.00,36,5,20,3,0.00,0.00),(33,0.00,38,5,16,3,0.00,0.00),(34,0.00,40,5,18,3,0.00,0.00),(35,0.00,42,5,34,3,0.00,0.00),(36,0.00,46,5,19,3,0.00,0.00),(37,800.00,62,5,1,3,0.00,800.00),(39,0.00,68,5,1,3,0.00,0.00),(40,0.00,69,5,9,3,0.00,0.00),(41,2500.00,74,5,2,3,125.00,2375.00);
/*!40000 ALTER TABLE `Order` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `OrderServices`
--

DROP TABLE IF EXISTS `OrderServices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `OrderServices` (
  `OrderId` int NOT NULL,
  `ServicesId` int NOT NULL,
  PRIMARY KEY (`OrderId`,`ServicesId`),
  KEY `fk_services_idx` (`ServicesId`),
  CONSTRAINT `fk_order` FOREIGN KEY (`OrderId`) REFERENCES `Order` (`idOrder`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_services` FOREIGN KEY (`ServicesId`) REFERENCES `Services` (`idServices`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `OrderServices`
--

LOCK TABLES `OrderServices` WRITE;
/*!40000 ALTER TABLE `OrderServices` DISABLE KEYS */;
INSERT INTO `OrderServices` VALUES (12,1),(31,1),(37,1),(7,3),(10,3),(23,3),(7,5),(12,5),(19,5),(14,6),(16,6),(17,6),(10,7),(13,7),(15,7),(11,11),(14,22),(17,22),(18,29),(18,35),(19,35),(13,37),(8,39),(15,49),(41,49),(6,53),(16,53);
/*!40000 ALTER TABLE `OrderServices` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Patients`
--

DROP TABLE IF EXISTS `Patients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Patients` (
  `idPatients` int NOT NULL AUTO_INCREMENT,
  `Surname` varchar(45) NOT NULL,
  `Name` varchar(45) NOT NULL,
  `Lastname` varchar(45) DEFAULT NULL,
  `Date_birth` date NOT NULL,
  `Phone_number` varchar(45) NOT NULL,
  `Number_policy` varchar(16) NOT NULL,
  PRIMARY KEY (`idPatients`)
) ENGINE=InnoDB AUTO_INCREMENT=56 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Patients`
--

LOCK TABLES `Patients` WRITE;
/*!40000 ALTER TABLE `Patients` DISABLE KEYS */;
INSERT INTO `Patients` VALUES (1,'Абрамов','Иван','Сергеевич','1990-01-12','+7 999 111 00 01','2643746275671247'),(2,'Алексеева','Мария','Игоревна','1985-05-22','+7 999 111 00 02','1232436235645324'),(3,'Андреев','Дмитрий','Петрович','1992-07-14','+7 999 111 00 03','3264736817476324'),(4,'Антонова','Екатерина','Андреевна','1995-09-03','+7 999 111 00 04','3247387597294832'),(5,'Белов','Станислав','Николаевич','1988-03-18','+7 999 111 00 05','2843673269198042'),(6,'Борисова','Ольга','Фёдоровна','1991-11-09','+7 999 111 00 06','3247278743577545'),(7,'Васильев','Андрей','Алексеевич','1993-02-15','+7 999 111 00 07','1478487987385873'),(8,'Волкова','Наталья','Павловна','1986-08-27','+7 999 111 00 08','3242758910949108'),(9,'Громов','Максим','Олегович','1990-06-30','+7 999 111 00 09','3274982759872874'),(10,'Гаврилова','Елена','Игоревна','1987-12-19','+7 999 111 00 10','0978673248637252'),(16,'Данилов','Артём','Сергеевич','1994-04-12','+7 999 111 00 11','3247569871234567'),(17,'Ефимова','София','Александровна','1989-09-25','+7 999 111 00 12','9876543210987654'),(18,'Жуков','Илья','Владимирович','1991-01-30','+7 999 111 00 13','1234987654321765'),(19,'Зайцева','Марина','Павловна','1992-03-21','+7 999 111 00 14','5647382910456789'),(20,'Иванов','Владимир','Александрович','1986-07-17','+7 999 111 00 15','9876123456784321'),(21,'Кузнецова','Анна','Игоревна','1995-12-05','+7 999 111 00 16','3456789012345678'),(22,'Лебедев','Никита','Дмитриевич','1990-10-08','+7 999 111 00 17','5678901234567890'),(23,'Морозова','Екатерина','Сергеевна','1988-06-22','+7 999 111 00 18','6789012345678901'),(24,'Николаев','Александр','Павлович','1993-11-15','+7 999 111 00 19','7890123456789012'),(25,'Орлова','Полина','Владимировна','1992-05-27','+7 999 111 00 20','8901234567890123'),(26,'Павлов','Денис','Сергеевич','1987-08-10','+7 999 111 00 21','9012345678901234'),(27,'Петрова','Вероника','Александровна','1994-02-14','+7 999 111 00 22','0123456789012345'),(28,'Романов','Константин','Игоревич','1991-09-30','+7 999 111 00 23','1234509876543210'),(29,'Семенова','Людмила','Дмитриевна','1985-03-19','+7 999 111 00 24','2345612345678901'),(30,'Соколов','Максим','Алексеевич','1990-12-02','+7 999 111 00 25','3456723456789012'),(31,'Тихонова','Маргарита','Сергеевна','1993-07-21','+7 999 111 00 26','4567834567890123'),(32,'Федоров','Павел','Николаевич','1989-05-11','+7 999 111 00 27','5678945678901234'),(33,'Фролова','Ирина','Андреевна','1992-08-03','+7 999 111 00 28','6789056789012345'),(34,'Харитонов','Виктор','Сергеевич','1990-11-18','+7 999 111 00 29','7890167890123456'),(35,'Чернова','Анна','Владимировна','1988-01-22','+7 999 111 00 30','8901278901234567'),(36,'Шестаков','Дмитрий','Александрович','1995-04-06','+7 999 111 00 31','9012389012345678'),(37,'Щербакова','Юлия','Игоревна','1991-06-15','+7 999 111 00 32','0123490123456789'),(38,'Юдин','Сергей','Владимирович','1987-09-28','+7 999 111 00 33','1234501234567890'),(39,'Яковлева','Ксения','Павловна','1994-02-19','+7 999 111 00 34','2345612345678902'),(40,'Баранов','Николай','Сергеевич','1990-03-07','+7 999 111 00 35','3456723456789013'),(41,'Васильева','Мария','Алексеевна','1989-10-23','+7 999 111 00 36','4567834567890124'),(42,'Горбунов','Илья','Дмитриевич','1992-12-14','+7 999 111 00 37','5678945678901235'),(43,'Демидова','Светлана','Игоревна','1986-05-05','+7 999 111 00 38','6789056789012346'),(44,'Егоров','Владислав','Павлович','1993-08-29','+7 999 111 00 39','7890167890123457'),(45,'Журавлёва','Ольга','Сергеевна','1991-11-12','+7 999 111 00 40','8901278901234568'),(46,'Ильин','Алексей','Павлович','1988-07-12','+7 999 111 00 41','9012389012345679'),(47,'Князева','Екатерина','Сергеевна','1990-01-25','+7 999 111 00 42','0123490123456790'),(48,'Лавров','Станислав','Игоревич','1992-03-18','+7 999 111 00 43','1234501234567891'),(49,'Макарова','Надежда','Владимировна','1989-09-05','+7 999 111 00 44','2345612345678903'),(50,'Носков','Денис','Александрович','1991-06-22','+7 999 111 00 45','3456723456789014'),(51,'Орлова','Светлана','Павловна','1993-12-11','+7 999 111 00 46','4567834567890125'),(52,'Панин','Игорь','Сергеевич','1987-02-16','+7 999 111 00 47','5678945678901236'),(53,'Рогова','Алина','Дмитриевна','1994-08-30','+7 999 111 00 48','6789056789012347'),(54,'Сидоров','Павел','Николаевич','1990-05-09','+7 999 111 00 49','7890167890123458'),(55,'Терехова','Анна','Алексеевна','1992-11-21','+7 999 111 00 50','8901278901234569');
/*!40000 ALTER TABLE `Patients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Roles`
--

DROP TABLE IF EXISTS `Roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Roles` (
  `idRoles` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(45) NOT NULL,
  PRIMARY KEY (`idRoles`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Roles`
--

LOCK TABLES `Roles` WRITE;
/*!40000 ALTER TABLE `Roles` DISABLE KEYS */;
INSERT INTO `Roles` VALUES (1,'Администратор'),(2,'Регистратор'),(3,'Главный врач');
/*!40000 ALTER TABLE `Roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Schedule`
--

DROP TABLE IF EXISTS `Schedule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Schedule` (
  `idSchedule` int NOT NULL AUTO_INCREMENT,
  `idDoctor` int NOT NULL,
  `Date` date NOT NULL,
  `Time` time NOT NULL,
  `Status` int NOT NULL,
  PRIMARY KEY (`idSchedule`),
  KEY `fk_Schedule_Statuses1_idx` (`Status`),
  KEY `fk_Schedule_Doctors1_idx` (`idDoctor`),
  CONSTRAINT `fk_Schedule_Doctors1` FOREIGN KEY (`idDoctor`) REFERENCES `Doctors` (`idDoctors`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_Schedule_Statuses1` FOREIGN KEY (`Status`) REFERENCES `Statuses` (`idStatuses`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Schedule`
--

LOCK TABLES `Schedule` WRITE;
/*!40000 ALTER TABLE `Schedule` DISABLE KEYS */;
INSERT INTO `Schedule` VALUES (1,1,'2025-09-26','09:00:00',2),(2,1,'2025-09-26','10:00:00',2),(3,2,'2025-09-26','11:00:00',2),(4,2,'2025-09-26','12:00:00',2),(5,3,'2025-09-27','09:30:00',2),(6,4,'2025-09-27','10:30:00',2),(7,5,'2025-09-27','11:30:00',2),(8,1,'2025-10-24','18:19:00',2),(9,5,'2025-10-30','20:00:00',2),(10,1,'2025-11-04','18:19:00',2),(12,2,'2025-09-28','09:00:00',2),(13,2,'2025-09-28','10:00:00',2),(14,3,'2025-09-28','11:00:00',2),(15,3,'2025-09-28','12:00:00',2),(16,4,'2025-09-29','09:30:00',2),(17,4,'2025-09-29','10:30:00',2),(18,5,'2025-09-29','11:30:00',2),(19,5,'2025-09-29','12:30:00',2),(20,6,'2025-09-30','09:00:00',2),(21,6,'2025-09-30','10:00:00',2),(22,7,'2025-09-30','11:00:00',2),(23,7,'2025-09-30','12:00:00',2),(24,1,'2025-10-01','09:00:00',2),(25,1,'2025-10-01','10:00:00',2),(26,2,'2025-10-01','11:00:00',2),(27,2,'2025-10-01','12:00:00',2),(28,3,'2025-10-02','09:30:00',2),(29,3,'2025-10-02','10:30:00',2),(30,4,'2025-10-02','11:30:00',2),(31,4,'2025-10-02','12:30:00',2),(32,5,'2025-10-03','09:00:00',2),(33,5,'2025-10-03','10:00:00',2),(34,6,'2025-10-03','11:00:00',2),(35,6,'2025-10-03','12:00:00',2),(36,7,'2025-10-04','09:30:00',2),(37,7,'2025-10-04','10:30:00',2),(38,1,'2025-10-04','11:30:00',2),(39,1,'2025-10-04','12:30:00',2),(40,2,'2025-10-05','09:00:00',2),(41,2,'2025-10-05','10:00:00',2),(42,3,'2025-10-05','11:00:00',2),(43,3,'2025-10-05','12:00:00',2),(44,4,'2025-10-06','09:30:00',2),(45,4,'2025-10-06','10:30:00',2),(46,5,'2025-10-06','11:30:00',2),(47,5,'2025-10-06','12:30:00',2),(49,6,'2025-10-07','10:00:00',2),(50,7,'2025-10-07','11:00:00',2),(51,7,'2025-10-07','12:00:00',2),(52,1,'2025-10-08','09:30:00',2),(53,1,'2025-10-08','10:30:00',2),(55,2,'2025-10-08','12:30:00',2),(56,3,'2025-10-09','09:00:00',2),(57,3,'2025-10-09','10:00:00',2),(58,4,'2025-10-09','11:00:00',2),(59,4,'2025-10-09','12:00:00',2),(62,7,'2025-12-08','18:19:00',2),(68,1,'2025-12-10','10:00:00',2),(69,1,'2025-12-10','12:00:00',2),(74,2,'2025-12-10','20:00:04',2);
/*!40000 ALTER TABLE `Schedule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Services`
--

DROP TABLE IF EXISTS `Services`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Services` (
  `idServices` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  `Price` decimal(10,2) NOT NULL,
  `Category` int NOT NULL,
  PRIMARY KEY (`idServices`),
  KEY `fk_Services_Category1_idx` (`Category`),
  CONSTRAINT `fk_Services_Category1` FOREIGN KEY (`Category`) REFERENCES `Category` (`idCategory`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=59 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Services`
--

LOCK TABLES `Services` WRITE;
/*!40000 ALTER TABLE `Services` DISABLE KEYS */;
INSERT INTO `Services` VALUES (1,'Первичная консультация стоматолога',800.00,1),(2,'Повторная консультация стоматолога',600.00,1),(3,'Рентген зуба',500.00,2),(5,'Лечение кариеса',2500.00,3),(6,'Пломбирование зуба',3000.00,3),(7,'Эндодонтическое лечение (каналы)',5000.00,3),(8,'Удаление зуба простое',3500.00,4),(9,'Удаление зуба сложное',6000.00,4),(10,'Протезирование (коронка)',12000.00,5),(11,'Установка брекетов',40000.00,6),(12,'Снятие брекетов',5000.00,6),(13,'Профессиональная чистка зубов',3500.00,7),(14,'Отбеливание зубов',15000.00,7),(15,'Детский приём стоматолога',2000.00,8),(20,'Консультация ортопеда',1000.00,1),(21,'Удаление зубного камня ультразвуком',4000.00,7),(22,'Лечение пульпита',4500.00,3),(23,'Отбеливание Zoom',20000.00,7),(24,'Имплантация зуба',30000.00,5),(25,'Коррекция прикуса пластинкой',15000.00,6),(26,'Детская профессиональная чистка',2500.00,8),(27,'Консультация хирурга-стоматолога',1200.00,1),(28,'Снятие зубного камня вручную',2500.00,7),(29,'Реставрация зуба композитом',3500.00,3),(30,'Протезирование (съёмный протез)',18000.00,5),(31,'Фторирование зубов',1500.00,7),(32,'Установка ретейнера',6000.00,6),(33,'Удаление зуба мудрости',8000.00,4),(34,'Консультация детского стоматолога с осмотром',2200.00,8),(35,'Консультация пародонтолога',1500.00,1),(36,'Лечение гингивита',3000.00,3),(37,'Рентген панорамный',1200.00,2),(38,'Удаление зуба с осложнениями',7000.00,4),(39,'Установка виниров',18000.00,5),(40,'Коррекция брекетов',3500.00,6),(41,'Чистка межзубных промежутков',2000.00,7),(42,'Детская фторизация зубов',1800.00,8),(43,'Повторная эндодонтическая обработка',5200.00,3),(44,'Снятие зубного камня + полировка',4500.00,7),(45,'Лечение периодонтита',5500.00,3),(46,'Установка временной коронки',4000.00,5),(47,'Снятие брекетов с чисткой',6000.00,6),(48,'Экстренное удаление зуба',8000.00,4),(49,'Консультация по имплантации',2500.00,1),(50,'Профилактическая обработка десен',3000.00,7),(51,'Лечение кариеса с использованием микроскопа',4000.00,3),(52,'Установка детских брекетов',25000.00,6),(53,'Консультация по ортопедическому лечению',2000.00,1),(54,'Протезирование мостовидным протезом',22000.00,5),(55,'Удаление молочного зуба',1200.00,8);
/*!40000 ALTER TABLE `Services` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Speciality`
--

DROP TABLE IF EXISTS `Speciality`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Speciality` (
  `idSpeciality` int NOT NULL AUTO_INCREMENT,
  `SpecialityName` varchar(45) NOT NULL,
  PRIMARY KEY (`idSpeciality`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Speciality`
--

LOCK TABLES `Speciality` WRITE;
/*!40000 ALTER TABLE `Speciality` DISABLE KEYS */;
INSERT INTO `Speciality` VALUES (1,'Терапевт-стоматолог'),(2,'Хирург-стоматолог'),(3,'Ортопед-стоматолог'),(4,'Ортодонт'),(5,'Детский стоматолог'),(6,'Стоматолог-гигиенист');
/*!40000 ALTER TABLE `Speciality` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Statuses`
--

DROP TABLE IF EXISTS `Statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Statuses` (
  `idStatuses` int NOT NULL AUTO_INCREMENT,
  `StatusName` varchar(45) NOT NULL,
  PRIMARY KEY (`idStatuses`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Statuses`
--

LOCK TABLES `Statuses` WRITE;
/*!40000 ALTER TABLE `Statuses` DISABLE KEYS */;
INSERT INTO `Statuses` VALUES (1,'Свободно'),(2,'Занято');
/*!40000 ALTER TABLE `Statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `StatusesPriem`
--

DROP TABLE IF EXISTS `StatusesPriem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `StatusesPriem` (
  `idStatusesPriem` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  PRIMARY KEY (`idStatusesPriem`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `StatusesPriem`
--

LOCK TABLES `StatusesPriem` WRITE;
/*!40000 ALTER TABLE `StatusesPriem` DISABLE KEYS */;
INSERT INTO `StatusesPriem` VALUES (1,'Завершен'),(2,'Отменен'),(3,'Создан');
/*!40000 ALTER TABLE `StatusesPriem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Users` (
  `idUsers` int NOT NULL AUTO_INCREMENT,
  `Surname` varchar(45) NOT NULL,
  `Name` varchar(45) NOT NULL,
  `Lastname` varchar(45) DEFAULT NULL,
  `Login` varchar(45) NOT NULL,
  `Password` varchar(256) NOT NULL,
  `Role` int NOT NULL,
  PRIMARY KEY (`idUsers`),
  KEY `fk_Users_Roles_idx` (`Role`),
  CONSTRAINT `fk_Users_Roles` FOREIGN KEY (`Role`) REFERENCES `Roles` (`idRoles`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Users`
--

LOCK TABLES `Users` WRITE;
/*!40000 ALTER TABLE `Users` DISABLE KEYS */;
INSERT INTO `Users` VALUES (1,'Иванов','Иван','Иванович','admin','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b',1),(2,'Сидоров','Алексей','Петрович','3','4e07408562bedb8b60ce05c1decfe3ad16b72230967de01f640b7e4729b49fce',3),(3,'Петрова','Мария','Сергеевна','reg1','dd5285466e782e305c3e0afccfd738c449e6fd3a1da8c7f711ede096565feb03',2),(4,'Кузнецова','Анна','Владимировна','1','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b',1),(5,'Морозов','Дмитрий','Олегович','2','d4735e3a265e16eee03f59718b9b5d03019c07d8b6c51f90da3a666eec13ab35',2),(6,'Фёдоров','Сергей','Ильич','reg4','d4735e3a265e16eee03f59718b9b5d03019c07d8b6c51f90da3a666eec13ab35',2),(7,'Козлова','Ольга','Викторовна','reg5','d4735e3a265e16eee03f59718b9b5d03019c07d8b6c51f90da3a666eec13ab35',2),(8,'Громов','Александр','Николаевич','reg6','d4735e3a265e16eee03f59718b9b5d03019c07d8b6c51f90da3a666eec13ab35',2),(9,'Васильева','Екатерина','Игоревна','reg7','d4735e3a265e16eee03f59718b9b5d03019c07d8b6c51f90da3a666eec13ab35',2),(10,'Соколова','Юлия','Андреевна','reg8','d4735e3a265e16eee03f59718b9b5d03019c07d8b6c51f90da3a666eec13ab35',2),(11,'Чирков','Дмитрий','Александрович','admin2','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b',1);
/*!40000 ALTER TABLE `Users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'vkr'
--

--
-- Dumping routines for database 'vkr'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-12-10 18:06:44
