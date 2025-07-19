CREATE DATABASE `globalcombat` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

DROP TABLE IF EXISTS `globalcombat`.`account`;
CREATE TABLE  `globalcombat`.`account` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(30) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `password` varchar(30) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `email` varchar(255) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `cc_info` varchar(255) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `visible_cc_info` varchar(255) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `info_visible` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'True',
  `wins` smallint(8) unsigned NOT NULL DEFAULT '0',
  `games` smallint(8) unsigned NOT NULL DEFAULT '0',
  `last_on` int(11) NOT NULL DEFAULT '0',
  `num_logins` int(11) NOT NULL DEFAULT '0',
  `session_id` int(11) NOT NULL DEFAULT '0',
  `session_exp` int(11) NOT NULL DEFAULT '0',
  `last_ip` varchar(15) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '0',
  `signed_up` int(11) NOT NULL DEFAULT '0',
  `status` enum('Civilian','Enlisted','Comissioned','Admin','SuperAdmin','Discharged','Disabled') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'Civilian',
  `disabled_by` mediumint(8) NOT NULL DEFAULT '0',
  `forward_emails` enum('GameStarts','AllGame','All','None','GameAll') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'All',
  `OptOut` int(11) NOT NULL DEFAULT '0',
  `OptOutKey` int(11) NOT NULL DEFAULT '0',
  `rating` smallint(11) unsigned NOT NULL DEFAULT '8500',
  `referred_by` mediumint(8) NOT NULL DEFAULT '0',
  `admin` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'False',
  PRIMARY KEY (`id`),
  KEY `rating` (`rating`),
  KEY `games` (`games`),
  KEY `name` (`name`),
  KEY `session_exp` (`session_exp`)
) ENGINE=InnoDB AUTO_INCREMENT=99371 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

DROP TABLE IF EXISTS `globalcombat`.`account_login`;
CREATE TABLE  `globalcombat`.`account_login` (
  `account_id` int(11) NOT NULL DEFAULT '0',
  `datetime` int(11) NOT NULL DEFAULT '0',
  `ipaddress` varchar(255) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `browser` varchar(255) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `adminused` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'True',
  PRIMARY KEY (`account_id`,`datetime`),
  KEY `ipaddress` (`ipaddress`),
  KEY `Index_3` (`datetime`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `globalcombat`.`area`;
CREATE TABLE  `globalcombat`.`area` (
  `game_id` int(11) NOT NULL DEFAULT '0',
  `area` int(11) NOT NULL DEFAULT '0',
  `owner_id` int(11) NOT NULL DEFAULT '0',
  `armies` int(11) NOT NULL DEFAULT '0',
  `com` int(11) NOT NULL DEFAULT '0',
  `com_target` int(11) NOT NULL DEFAULT '0',
  `com_amount` int(11) NOT NULL DEFAULT '0',
  `new_armies` int(11) NOT NULL DEFAULT '0',
  `region` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`game_id`,`area`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `globalcombat`.`cheat`;
CREATE TABLE  `globalcombat`.`cheat` (
  `account_id` int(10) unsigned NOT NULL DEFAULT '0',
  `game_id` int(10) unsigned NOT NULL DEFAULT '0',
  `datetime` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`account_id`,`datetime`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

DROP TABLE IF EXISTS `globalcombat`.`game`;
CREATE TABLE  `globalcombat`.`game` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `game_name` varchar(30) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `start_time` int(11) NOT NULL DEFAULT '0',
  `end_time` int(11) NOT NULL DEFAULT '0',
  `status` int(11) NOT NULL DEFAULT '0',
  `owner_num` mediumint(8) NOT NULL DEFAULT '0',
  `armies` int(11) NOT NULL DEFAULT '0',
  `map_name` varchar(30) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'original',
  `max_players` tinyint(4) NOT NULL DEFAULT '2',
  `created_time` int(11) NOT NULL DEFAULT '0',
  `cur_players` tinyint(11) NOT NULL DEFAULT '0',
  `turn` smallint(4) NOT NULL DEFAULT '1',
  `prev_turn_time` int(11) NOT NULL DEFAULT '0',
  `last_turn_time` int(11) NOT NULL DEFAULT '0',
  `turn_length` smallint(5) NOT NULL DEFAULT '5',
  `passkey` int(11) NOT NULL DEFAULT '0',
  `fogged` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'False',
  `min_armies` tinyint(4) NOT NULL DEFAULT '0',
  `min_ranking` smallint(6) NOT NULL DEFAULT '0',
  `attack_order` enum('Largest','Smallest') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'Largest',
  `realtime` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'False',
  `tourney_id` mediumint(8) NOT NULL DEFAULT '0',
  `config_string` varchar(255) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`id`),
  KEY `created_time` (`created_time`),
  KEY `status` (`status`)
) ENGINE=InnoDB AUTO_INCREMENT=684316 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

DROP TABLE IF EXISTS `globalcombat`.`ledger`;
CREATE TABLE  `globalcombat`.`ledger` (
  `id` mediumint(8) unsigned NOT NULL AUTO_INCREMENT,
  `account` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `status` enum('Pending','Paid','Declined','Canceled') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'Pending',
  `type` enum('Deposit','Withdrawal','Prize','Enlistment Fee','Tourney Fee','Game Fee') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'Deposit',
  `item_id` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `time` int(11) NOT NULL DEFAULT '0',
  `amount` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `Account` (`account`)
) ENGINE=InnoDB AUTO_INCREMENT=7189 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

DROP TABLE IF EXISTS `globalcombat`.`message`;
CREATE TABLE  `globalcombat`.`message` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `to_id` int(11) NOT NULL DEFAULT '0',
  `from_id` int(11) NOT NULL DEFAULT '0',
  `time` int(11) NOT NULL DEFAULT '0',
  `text` text CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL,
  `readflag` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'False',
  `deleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `time` (`time`),
  KEY `to_id` (`to_id`),
  KEY `Index_4` (`from_id`)
) ENGINE=InnoDB AUTO_INCREMENT=14292785 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

DROP TABLE IF EXISTS `globalcombat`.`player`;
CREATE TABLE  `globalcombat`.`player` (
  `game_id` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `id` int(8) NOT NULL DEFAULT '0',
  `name` char(30) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `player_num` tinyint(8) unsigned NOT NULL DEFAULT '0',
  `prev_move` smallint(8) unsigned DEFAULT NULL,
  `last_move` smallint(8) unsigned DEFAULT NULL,
  `done` tinyint(4) NOT NULL DEFAULT '0',
  `new_armies` int(11) NOT NULL DEFAULT '0',
  `armies` int(11) NOT NULL DEFAULT '0',
  `areas` tinyint(8) unsigned NOT NULL DEFAULT '0',
  `status` tinyint(4) NOT NULL DEFAULT '0',
  `score_expected` float(7,5) NOT NULL DEFAULT '0.50000',
  `score` float(7,5) NOT NULL DEFAULT '0.00000',
  `rating` smallint(11) unsigned NOT NULL DEFAULT '0',
  `rating_change` smallint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`game_id`,`player_num`),
  KEY `id` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `globalcombat`.`tourney`;
CREATE TABLE  `globalcombat`.`tourney` (
  `id` mediumint(8) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `description` text CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL,
  `status` enum('New','Running','Finished') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'New',
  `players` smallint(5) NOT NULL DEFAULT '0',
  `curplayers` smallint(5) NOT NULL DEFAULT '0',
  `create_time` int(10) NOT NULL DEFAULT '0',
  `start_time` int(10) NOT NULL DEFAULT '0',
  `end_time` int(10) NOT NULL DEFAULT '0',
  `gamesize` tinyint(3) NOT NULL DEFAULT '0',
  `winners` tinyint(3) NOT NULL DEFAULT '0',
  `doubleelim` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'True',
  `kitty` smallint(5) NOT NULL DEFAULT '0',
  `cost` smallint(6) NOT NULL DEFAULT '0',
  `Options` varchar(255) CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT '',
  `AutoStart` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'False',
  `Recurring` enum('True','False') CHARACTER SET latin1 COLLATE latin1_general_ci NOT NULL DEFAULT 'False',
  `OptionGameId` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4978 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

DROP TABLE IF EXISTS `globalcombat`.`tourneygame`;
CREATE TABLE  `globalcombat`.`tourneygame` (
  `tourney_id` int(11) NOT NULL DEFAULT '0',
  `game_id` int(11) NOT NULL DEFAULT '0',
  `game_num` int(11) NOT NULL DEFAULT '0',
  `round` int(11) NOT NULL DEFAULT '0',
  `winners` int(11) NOT NULL DEFAULT '2',
  `winner_round` int(11) NOT NULL DEFAULT '0',
  `loser_round` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`game_id`),
  KEY `tourney_id` (`tourney_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

DROP TABLE IF EXISTS `globalcombat`.`tourneyplayer`;
CREATE TABLE  `globalcombat`.`tourneyplayer` (
  `tourney_id` mediumint(8) NOT NULL DEFAULT '0',
  `account_id` mediumint(8) NOT NULL DEFAULT '0',
  PRIMARY KEY (`tourney_id`,`account_id`),
  KEY `Accounts` (`account_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci COMMENT='InnoDB free: 6144 kB';