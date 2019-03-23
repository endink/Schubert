	create table `user_token` (
                        `id` bigint NOT NULL primary key,
                        `user_id` bigint NOT NULL,
                        `login_provider` varchar(16) NOT NULL,
                        `name` varchar(32) NOT NULL,
                        `value` varchar(128) NULL,
	                    constraint uk_userid_loginprovider_name unique(`user_id`,`login_provider`,`name`));
    create table `user_login` (
	                `id` bigint NOT NULL primary key,
                    `login_provider`       varchar(16) NOT NULL,
                    `provider_key`         varchar(64) NOT NULL,
                    `provider_display_name` varchar(16) NOT NULL,
                    `user_id`              bigint NOT NULL,
	                constraint uk_loginprovider_providerkey unique(`login_provider`,`provider_key`));
    create table `user_claim` (
                    `id`         bigint NOT NULL primary key,
                    `claim_type`  varchar(32) NULL,
                    `claim_value` varchar(64) NULL,
                    `user_id`     bigint NOT NULL);
    create table`user_base` (
                    `id`                   bigint NOT NULL primary key,
                    `access_failed_count`    INT NOT NULL,
                    `email`                varchar(64)     NULL,
                    `email_confirmed`       bit NOT NULL,
                    `language`             varchar(32)     NOT NULL,
                    `lockout_enabled`       bit NOT NULL,
                    `lockout_end`           datetime NULL,
                    `normalized_user_name`   varchar(16)    NOT NULL,
                    `password_hash`         varchar(256)     NOT NULL,
                    `phone_number`          varchar(18)     NULL unique,
                    `phone_number_confirmed` bit NOT NULL,
                    `security_stamp`        varchar(32)     NULL,
                    `time_zone`             varchar(32)     NULL,
                    `user_name`             varchar(16)     NOT NULL,
                    constraint uk_username unique(`user_name`),
	                constraint uk_phonenumber unique(`phone_number`),
	                constraint uk_email unique(`email`));
 create table `user_role` (
                          `user_id` bigint NOT NULL,
                          `role_id` bigint(20) NOT NULL,
                          `role_name` varchar(16) NOT NULL,
                          PRIMARY KEY(`user_id`,`role_id`));
create table `role_claim` (
                          `id` bigint NOT NULL primary key,
                          `role_id` bigint(20) NOT NULL,
                          `claim_type` varchar (32) NULL,
                          `claim_value` varchar (64) NULL)  ;
