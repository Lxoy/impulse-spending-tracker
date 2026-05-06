-- MySQL dump 10.13  Distrib 9.7.0, for Linux (x86_64)
--
-- Host: localhost    Database: impulse_db
-- ------------------------------------------------------
-- Server version	9.7.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
SET @MYSQLDUMP_TEMP_LOG_BIN = @@SESSION.SQL_LOG_BIN;
SET @@SESSION.SQL_LOG_BIN= 0;

--
-- GTID state at the beginning of the backup 
--

SET @@GLOBAL.GTID_PURGED=/*!80000 '+'*/ '1a5a76d8-3f39-11f1-82a6-f2d96659067b:1-103';

--
-- Table structure for table `BudgetPlans`
--

DROP TABLE IF EXISTS `BudgetPlans`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `BudgetPlans` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserProfileId` int NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ValidFrom` datetime(6) NOT NULL,
  `ValidTo` datetime(6) NOT NULL,
  `MonthlyLimit` decimal(65,30) NOT NULL,
  `ImpulseCapPercentage` double NOT NULL,
  `EssentialCategoryLimit` decimal(65,30) NOT NULL,
  `DiscretionaryCategoryLimit` decimal(65,30) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_BudgetPlans_UserProfileId` (`UserProfileId`),
  CONSTRAINT `FK_BudgetPlans_UserProfiles_UserProfileId` FOREIGN KEY (`UserProfileId`) REFERENCES `UserProfiles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `BudgetPlans`
--

LOCK TABLES `BudgetPlans` WRITE;
/*!40000 ALTER TABLE `BudgetPlans` DISABLE KEYS */;
INSERT INTO `BudgetPlans` VALUES (1,1,'Ana Q1 2025','2025-01-01 00:00:00.000000','2025-03-31 00:00:00.000000',1500.000000000000000000000000000000,15,800.000000000000000000000000000000,400.000000000000000000000000000000,0),(2,2,'Marko Annual','2025-01-01 00:00:00.000000','2025-12-31 00:00:00.000000',2000.000000000000000000000000000000,20,1000.000000000000000000000000000000,600.000000000000000000000000000000,1),(3,3,'Petra Savings','2025-03-01 00:00:00.000000','2025-06-30 00:00:00.000000',1000.000000000000000000000000000000,10,600.000000000000000000000000000000,200.000000000000000000000000000000,1);
/*!40000 ALTER TABLE `BudgetPlans` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Merchants`
--

DROP TABLE IF EXISTS `Merchants`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Merchants` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Category` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CountryCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsOnlineOnly` tinyint(1) NOT NULL,
  `AverageDeliveryDays` int DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Merchants`
--

LOCK TABLES `Merchants` WRITE;
/*!40000 ALTER TABLE `Merchants` DISABLE KEYS */;
INSERT INTO `Merchants` VALUES (1,'Amazon','Electronics','US',1,3),(2,'Zara','Fashion','ES',0,NULL),(3,'Steam','Gaming','US',1,0),(4,'Konzum','Groceries','HR',0,NULL);
/*!40000 ALTER TABLE `Merchants` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `PurchaseTags`
--

DROP TABLE IF EXISTS `PurchaseTags`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `PurchaseTags` (
  `PurchasesId` int NOT NULL,
  `TagsId` int NOT NULL,
  PRIMARY KEY (`PurchasesId`,`TagsId`),
  KEY `IX_PurchaseTags_TagsId` (`TagsId`),
  CONSTRAINT `FK_PurchaseTags_Purchases_PurchasesId` FOREIGN KEY (`PurchasesId`) REFERENCES `Purchases` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_PurchaseTags_Tags_TagsId` FOREIGN KEY (`TagsId`) REFERENCES `Tags` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `PurchaseTags`
--

LOCK TABLES `PurchaseTags` WRITE;
/*!40000 ALTER TABLE `PurchaseTags` DISABLE KEYS */;
INSERT INTO `PurchaseTags` VALUES (1,1),(2,1),(3,2),(2,3);
/*!40000 ALTER TABLE `PurchaseTags` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Purchases`
--

DROP TABLE IF EXISTS `Purchases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Purchases` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserProfileId` int NOT NULL,
  `MerchantId` int NOT NULL,
  `SpendingSessionId` int DEFAULT NULL,
  `BudgetPlanId` int DEFAULT NULL,
  `WishlistItemId` int DEFAULT NULL,
  `Title` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Amount` decimal(65,30) NOT NULL,
  `Currency` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PurchasedAt` datetime(6) NOT NULL,
  `MoodBeforePurchase` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `NeedLevel` int NOT NULL,
  `TriggerType` int NOT NULL,
  `Installments` int NOT NULL,
  `Notes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Purchases_WishlistItemId` (`WishlistItemId`),
  KEY `IX_Purchases_BudgetPlanId` (`BudgetPlanId`),
  KEY `IX_Purchases_MerchantId` (`MerchantId`),
  KEY `IX_Purchases_SpendingSessionId` (`SpendingSessionId`),
  KEY `IX_Purchases_UserProfileId` (`UserProfileId`),
  CONSTRAINT `FK_Purchases_BudgetPlans_BudgetPlanId` FOREIGN KEY (`BudgetPlanId`) REFERENCES `BudgetPlans` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Purchases_Merchants_MerchantId` FOREIGN KEY (`MerchantId`) REFERENCES `Merchants` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Purchases_SpendingSessions_SpendingSessionId` FOREIGN KEY (`SpendingSessionId`) REFERENCES `SpendingSessions` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Purchases_UserProfiles_UserProfileId` FOREIGN KEY (`UserProfileId`) REFERENCES `UserProfiles` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Purchases_WishlistItems_WishlistItemId` FOREIGN KEY (`WishlistItemId`) REFERENCES `WishlistItems` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Purchases`
--

LOCK TABLES `Purchases` WRITE;
/*!40000 ALTER TABLE `Purchases` DISABLE KEYS */;
INSERT INTO `Purchases` VALUES (1,1,1,1,1,NULL,'Bluetooth Headphones',74.990000000000000000000000000000,'EUR','2025-02-10 19:00:00.000000','Happy',3,1,1,'30% discount'),(2,2,3,2,2,2,'The Witcher 3 Complete',29.990000000000000000000000000000,'EUR','2025-03-05 21:30:00.000000','Bored',2,2,1,NULL),(3,3,2,NULL,3,3,'Summer Dress',49.990000000000000000000000000000,'EUR','2025-04-15 11:00:00.000000','Excited',4,3,1,'Sale -20%');
/*!40000 ALTER TABLE `Purchases` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `SpendingSessions`
--

DROP TABLE IF EXISTS `SpendingSessions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `SpendingSessions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserProfileId` int NOT NULL,
  `StartedAt` datetime(6) NOT NULL,
  `EndedAt` datetime(6) DEFAULT NULL,
  `Platform` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Channel` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SessionBudget` decimal(65,30) NOT NULL,
  `SpentAmount` decimal(65,30) NOT NULL,
  `ItemsViewed` int NOT NULL,
  `ItemsAddedToCart` int NOT NULL,
  `CheckoutCompleted` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_SpendingSessions_UserProfileId` (`UserProfileId`),
  CONSTRAINT `FK_SpendingSessions_UserProfiles_UserProfileId` FOREIGN KEY (`UserProfileId`) REFERENCES `UserProfiles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `SpendingSessions`
--

LOCK TABLES `SpendingSessions` WRITE;
/*!40000 ALTER TABLE `SpendingSessions` DISABLE KEYS */;
INSERT INTO `SpendingSessions` VALUES (1,1,'2025-02-10 18:30:00.000000','2025-02-10 19:15:00.000000','Amazon','Mobile App',100.000000000000000000000000000000,74.990000000000000000000000000000,12,3,1),(2,2,'2025-03-05 21:00:00.000000','2025-03-05 21:45:00.000000','Steam','Desktop Web',50.000000000000000000000000000000,29.990000000000000000000000000000,5,2,1),(3,3,'2025-04-01 14:00:00.000000',NULL,'Zara','Mobile App',80.000000000000000000000000000000,0.000000000000000000000000000000,8,1,0);
/*!40000 ALTER TABLE `SpendingSessions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Tags`
--

DROP TABLE IF EXISTS `Tags`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Tags` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ColorHex` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Tags`
--

LOCK TABLES `Tags` WRITE;
/*!40000 ALTER TABLE `Tags` DISABLE KEYS */;
INSERT INTO `Tags` VALUES (1,'Impulse','#FF4444','Unplanned purchases'),(2,'Essential','#44BB44','Necessary purchases'),(3,'Luxury','#AA44FF','Luxury items');
/*!40000 ALTER TABLE `Tags` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UserProfiles`
--

DROP TABLE IF EXISTS `UserProfiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserProfiles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `FirstName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LastName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DateOfBirth` datetime(6) NOT NULL,
  `MonthlyNetIncome` decimal(65,30) NOT NULL,
  `RiskToleranceScore` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UserProfiles`
--

LOCK TABLES `UserProfiles` WRITE;
/*!40000 ALTER TABLE `UserProfiles` DISABLE KEYS */;
INSERT INTO `UserProfiles` VALUES (1,'Ana','Horvat','ana.horvat@email.com','1992-05-14 00:00:00.000000',2800.000000000000000000000000000000,6,'2026-05-06 14:43:59.000000'),(2,'Marko','Kovac','marko.kovac@email.com','1988-11-03 00:00:00.000000',3500.000000000000000000000000000000,8,'2026-05-06 14:43:59.000000'),(3,'Petra','Babic','petra.babic@email.com','1995-07-22 00:00:00.000000',2200.000000000000000000000000000000,4,'2026-05-06 14:43:59.000000');
/*!40000 ALTER TABLE `UserProfiles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `WishlistItems`
--

DROP TABLE IF EXISTS `WishlistItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `WishlistItems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserProfileId` int NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DesiredPrice` decimal(65,30) NOT NULL,
  `CurrentPrice` decimal(65,30) NOT NULL,
  `Priority` int NOT NULL,
  `AddedAt` datetime(6) NOT NULL,
  `TargetPurchaseDate` datetime(6) DEFAULT NULL,
  `Reason` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsPurchased` tinyint(1) NOT NULL,
  `LinkUrl` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_WishlistItems_UserProfileId` (`UserProfileId`),
  CONSTRAINT `FK_WishlistItems_UserProfiles_UserProfileId` FOREIGN KEY (`UserProfileId`) REFERENCES `UserProfiles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `WishlistItems`
--

LOCK TABLES `WishlistItems` WRITE;
/*!40000 ALTER TABLE `WishlistItems` DISABLE KEYS */;
INSERT INTO `WishlistItems` VALUES (1,1,'Sony WH-1000XM5',250.000000000000000000000000000000,319.990000000000000000000000000000,1,'2025-01-15 00:00:00.000000','2025-06-01 00:00:00.000000','For working from home',0,'https://amazon.com/sony-wh1000xm5'),(2,2,'The Witcher 3 DLC',15.000000000000000000000000000000,19.990000000000000000000000000000,3,'2025-02-01 00:00:00.000000',NULL,'Waiting for a sale',0,'https://store.steampowered.com'),(3,3,'Summer Dress Zara',40.000000000000000000000000000000,59.990000000000000000000000000000,2,'2025-03-20 00:00:00.000000','2025-05-01 00:00:00.000000','For vacation',0,'https://zara.com');
/*!40000 ALTER TABLE `WishlistItems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__EFMigrationsHistory`
--

LOCK TABLES `__EFMigrationsHistory` WRITE;
/*!40000 ALTER TABLE `__EFMigrationsHistory` DISABLE KEYS */;
INSERT INTO `__EFMigrationsHistory` VALUES ('20260423182139_InitialCreate','9.0.0');
/*!40000 ALTER TABLE `__EFMigrationsHistory` ENABLE KEYS */;
UNLOCK TABLES;
SET @@SESSION.SQL_LOG_BIN = @MYSQLDUMP_TEMP_LOG_BIN;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-05-06 14:44:19
