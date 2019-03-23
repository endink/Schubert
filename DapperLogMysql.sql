CREATE TABLE IF NOT EXISTS `Logs` (
                          `id` bigint NOT NULL primary key,
                          `application` varchar(32) NOT NULL,
                          `event_id` int NOT NULL default 0,
                          `level` int NOT NULL default 0,
                          `host` varchar(32) NULL,
                          `user` varchar(16) NULL,
                          `time_created` datetime NOT NULL,
                          `message` text NOT NULL,
                          `category` varchar(64) NOT NULL,
                          `app_version` varchar(64) NOT NULL);