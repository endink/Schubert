create table `UserTokens` (
                        `Id` bigint NOT NULL primary key,
                        `UserId` bigint NOT NULL,
                        `LoginProvider` varchar(16) NOT NULL,
                        `Name` varchar(32) NOT NULL,
                        `Value` varchar(128) NULL,
	                constraint UK_UserId_LoginProvider_Name unique(`UserId`,`LoginProvider`,`Name`))
			CHARACTER SET utf8
                        COLLATE utf8_general_ci;

create table `UseLogins` (
	                `Id` bigint NOT NULL primary key,
                    `LoginProvider`       varchar(16) NOT NULL,
                    `ProviderKey`         varchar(64) NOT NULL,
                    `ProviderDisplayName` varchar(16) NOT NULL,
                    `UserId`              bigint            NOT NULL,
	                constraint UK_LoginProvider_ProviderKey unique(`LoginProvider`,`ProviderKey`))
			CHARACTER SET utf8
                        COLLATE utf8_general_ci;

create table `UserClaims` (
                    `Id`         bigint   NOT NULL primary key,
                    `ClaimType`  varchar (32) NULL,
                    `ClaimValue` varchar (64) NULL,
                    `UserId`     bigint   NOT NULL)
			CHARACTER SET utf8
                        COLLATE utf8_general_ci;


Create TABLE `UserBases` (
                    `Id`                   bigint         NOT NULL primary key,
                    `AccessFailedCount`    INT                NOT NULL,
                    `Email`                varchar (64)     NULL,
                    `EmailConfirmed`       bit                NOT NULL,
                    `Language`             varchar(32)     NOT NULL,
                    `LockoutEnabled`       bit                NOT NULL,
                    `LockoutEnd`           datetime NULL,
                    `NormalizedUserName`   varchar(16)    NOT NULL,
                    `PasswordHash`         varchar(256)     NOT NULL,
                    `PhoneNumber`          varchar(18)     NULL unique,
                    `PhoneNumberConfirmed` bit                NOT NULL,
                    `SecurityStamp`        varchar(32)     NULL,
                    `TimeZone`             varchar(32)     NULL,
                    `UserName`             varchar(16)     NOT NULL,
	                constraint UK_UserName unique(`UserName`),
	                constraint UK_PhoneNumber unique(`PhoneNumber`),
	                constraint UK_Email unique(`Email`))
			CHARACTER SET utf8
                        COLLATE utf8_general_ci;

CREATE TABLE `UserRoles` (
                          `UserId` bigint(20) NOT NULL,
                          `RoleId` bigint(20) NOT NULL,
                          `RoleName` varchar(255) DEFAULT NULL,
                          PRIMARY KEY (`UserId`,`RoleId`))
		           CHARACTER SET utf8
                           COLLATE utf8_general_ci;

CREATE TABLE `RoleBases` (
                    `Id`         bigint   NOT NULL primary key,
                    `RoleName`   varchar (100) NOT NULL,
                    `DisplayName`  varchar (100)   NULL)
                     CHARACTER SET utf8
                     COLLATE utf8_general_ci;

CREATE TABLE `RoleClaims` (
                          `Id` bigint NOT NULL primary key,
                          `RoleId` bigint(20) NOT NULL,
                          `ClaimType` varchar (32) NULL,
                          `ClaimValue` varchar (64) NULL)
                           CHARACTER SET utf8
                           COLLATE utf8_general_ci;

