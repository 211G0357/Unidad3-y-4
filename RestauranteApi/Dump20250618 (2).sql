-- MySQL dump 10.13  Distrib 8.0.34, for Win64 (x86_64)
--
-- Host: localhost    Database: hamburguesa
-- ------------------------------------------------------
-- Server version	8.0.34

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
-- Table structure for table `hamburguesa`
--

DROP TABLE IF EXISTS `hamburguesa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hamburguesa` (
  `IdHamburguesa` int NOT NULL AUTO_INCREMENT,
  `Categoria` varchar(100) DEFAULT NULL,
  `Precio` decimal(8,2) DEFAULT NULL,
  PRIMARY KEY (`IdHamburguesa`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `hamburguesa`
--

LOCK TABLES `hamburguesa` WRITE;
/*!40000 ALTER TABLE `hamburguesa` DISABLE KEYS */;
INSERT INTO `hamburguesa` VALUES (1,'LaMiniBurguer',30.00),(2,'Sencilla',50.00),(3,'Doble',70.00),(4,'AlaPollo',55.00),(5,'Explosion de Queso',65.00),(6,'Colorburgue',80.00);
/*!40000 ALTER TABLE `hamburguesa` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `papas`
--

DROP TABLE IF EXISTS `papas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `papas` (
  `IdPapas` int NOT NULL AUTO_INCREMENT,
  `Categoria` varchar(100) DEFAULT NULL,
  `Precio` decimal(8,2) DEFAULT NULL,
  PRIMARY KEY (`IdPapas`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `papas`
--

LOCK TABLES `papas` WRITE;
/*!40000 ALTER TABLE `papas` DISABLE KEYS */;
INSERT INTO `papas` VALUES (1,'Chica',20.00),(2,'Mediana',25.00),(3,'Grande',30.00),(4,'Grande con queso',35.00),(5,'Especiales',40.00);
/*!40000 ALTER TABLE `papas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pedido`
--

DROP TABLE IF EXISTS `pedido`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pedido` (
  `IdPedido` int NOT NULL AUTO_INCREMENT,
  `NumMesa` int DEFAULT NULL,
  `Fecha` datetime DEFAULT CURRENT_TIMESTAMP,
  `IdUsuario` int DEFAULT NULL,
  `Estado` enum('Pendiente','Preparación','Terminado') DEFAULT 'Pendiente',
  PRIMARY KEY (`IdPedido`),
  KEY `IdUsuario` (`IdUsuario`),
  CONSTRAINT `pedido_ibfk_1` FOREIGN KEY (`IdUsuario`) REFERENCES `usuario` (`IdUsuario`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedido`
--

LOCK TABLES `pedido` WRITE;
/*!40000 ALTER TABLE `pedido` DISABLE KEYS */;
INSERT INTO `pedido` VALUES (1,1,'2025-06-16 23:36:32',2,'Terminado'),(2,2,'2025-06-16 23:36:32',2,'Terminado'),(3,3,'2025-06-16 23:36:32',3,'Terminado'),(14,1,'2025-06-18 14:41:27',2,'Pendiente');
/*!40000 ALTER TABLE `pedido` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pedidococina`
--

DROP TABLE IF EXISTS `pedidococina`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pedidococina` (
  `IdEstado` int NOT NULL AUTO_INCREMENT,
  `IdDetalle` int DEFAULT NULL,
  `Estado` enum('Pendiente','En preparación','Terminado') DEFAULT 'Pendiente',
  PRIMARY KEY (`IdEstado`),
  KEY `IdDetalle` (`IdDetalle`),
  CONSTRAINT `pedidococina_ibfk_1` FOREIGN KEY (`IdDetalle`) REFERENCES `pedidodetalle` (`IdDetalle`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedidococina`
--

LOCK TABLES `pedidococina` WRITE;
/*!40000 ALTER TABLE `pedidococina` DISABLE KEYS */;
INSERT INTO `pedidococina` VALUES (1,1,'Terminado'),(2,2,'Terminado'),(3,3,'Pendiente'),(4,4,'Terminado'),(5,5,'Terminado'),(6,6,'Pendiente'),(7,7,'Terminado'),(8,8,'En preparación'),(9,9,'Terminado'),(10,14,'Terminado'),(11,13,'Terminado'),(12,12,'Terminado'),(13,11,'Terminado'),(14,10,'Terminado'),(17,16,'Pendiente');
/*!40000 ALTER TABLE `pedidococina` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pedidodetalle`
--

DROP TABLE IF EXISTS `pedidodetalle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pedidodetalle` (
  `IdDetalle` int NOT NULL AUTO_INCREMENT,
  `IdPedido` int DEFAULT NULL,
  `TipoProducto` enum('Hamburguesa','Papas','Refresco') DEFAULT NULL,
  `IdProducto` int DEFAULT NULL,
  `Cantidad` int DEFAULT NULL,
  `PrecioUnitario` decimal(8,2) DEFAULT NULL,
  PRIMARY KEY (`IdDetalle`),
  KEY `IdPedido` (`IdPedido`),
  CONSTRAINT `pedidodetalle_ibfk_1` FOREIGN KEY (`IdPedido`) REFERENCES `pedido` (`IdPedido`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedidodetalle`
--

LOCK TABLES `pedidodetalle` WRITE;
/*!40000 ALTER TABLE `pedidodetalle` DISABLE KEYS */;
INSERT INTO `pedidodetalle` VALUES (1,1,'Hamburguesa',1,2,30.00),(2,1,'Papas',2,1,25.00),(3,1,'Refresco',13,1,15.00),(4,2,'Hamburguesa',3,1,70.00),(5,2,'Papas',4,1,35.00),(6,2,'Refresco',15,1,20.00),(7,3,'Hamburguesa',5,2,65.00),(8,3,'Refresco',14,2,18.00),(9,1,'Hamburguesa',2,3,50.00),(10,1,'Hamburguesa',2,3,50.00),(11,1,'Hamburguesa',2,3,50.00),(12,1,'Hamburguesa',2,3,50.00),(13,1,'Hamburguesa',2,3,50.00),(14,1,'Hamburguesa',2,1,50.00),(16,14,'Hamburguesa',2,1,50.00),(17,14,'Refresco',1,1,15.00);
/*!40000 ALTER TABLE `pedidodetalle` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `refrescoprecio`
--

DROP TABLE IF EXISTS `refrescoprecio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refrescoprecio` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `idSaboresRefresco` int DEFAULT NULL,
  `Tamaño` varchar(50) NOT NULL,
  `Precio` decimal(8,2) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `refrescoprecio`
--

LOCK TABLES `refrescoprecio` WRITE;
/*!40000 ALTER TABLE `refrescoprecio` DISABLE KEYS */;
INSERT INTO `refrescoprecio` VALUES (1,1,'Chico',15.00),(2,1,'Mediano',18.00),(3,1,'Grande',20.00),(4,2,'Chico',15.00),(5,2,'Mediano',18.00),(6,2,'Grande',20.00),(7,3,'Chico',15.00),(8,3,'Mediano',18.00),(9,3,'Grande',20.00),(10,4,'Chico',15.00),(11,4,'Mediano',18.00),(12,4,'Grande',20.00),(13,5,'Chico',15.00),(14,5,'Mediano',18.00),(15,5,'Grande',20.00),(16,6,'Chico',15.00),(17,6,'Mediano',18.00),(18,6,'Grande',20.00);
/*!40000 ALTER TABLE `refrescoprecio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `saboresrefresco`
--

DROP TABLE IF EXISTS `saboresrefresco`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `saboresrefresco` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Sabor` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `saboresrefresco`
--

LOCK TABLES `saboresrefresco` WRITE;
/*!40000 ALTER TABLE `saboresrefresco` DISABLE KEYS */;
INSERT INTO `saboresrefresco` VALUES (1,'Coca'),(2,'Fresa'),(3,'Naranja'),(4,'Sprite'),(5,'CocaLigth'),(6,'Manzana');
/*!40000 ALTER TABLE `saboresrefresco` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuario`
--

DROP TABLE IF EXISTS `usuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuario` (
  `IdUsuario` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) DEFAULT NULL,
  `Contraseña` varchar(255) NOT NULL,
  `Rol` enum('Mesero','Cocinero') NOT NULL,
  PRIMARY KEY (`IdUsuario`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuario`
--

LOCK TABLES `usuario` WRITE;
/*!40000 ALTER TABLE `usuario` DISABLE KEYS */;
INSERT INTO `usuario` VALUES (1,'Julio','1234','Cocinero'),(2,'Leo','A1234567a','Mesero'),(3,'Cris','A1234567aBa','Mesero'),(4,'Master','12345Aa','Mesero');
/*!40000 ALTER TABLE `usuario` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-06-18 15:01:53
