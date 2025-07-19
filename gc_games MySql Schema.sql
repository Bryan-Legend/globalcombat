CREATE DATABASE `gc_games` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

DROP TABLE IF EXISTS `gc_games`.`game`;
CREATE TABLE  `gc_games`.`game` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `status` int(11) NOT NULL DEFAULT '0',
  `serialized` longblob NOT NULL,
  `private` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `status` (`status`)
) ENGINE=InnoDB AUTO_INCREMENT=751207 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

DROP TABLE IF EXISTS `gc_games`.`player`;
CREATE TABLE  `gc_games`.`player` (
  `game_id` int(11) NOT NULL DEFAULT '0',
  `account_id` int(11) NOT NULL DEFAULT '0',
  `isInvite` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`account_id`,`game_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;