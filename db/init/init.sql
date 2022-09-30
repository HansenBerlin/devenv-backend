create database userdb;
use userdb;

create table serviceuser
(
	Id int auto_increment,
	Username TEXT(100) not null,
	Mail Text(100) not null,
	Age int not null,
	constraint serviceuser_pk
		primary key (Id)
);

INSERT INTO userdb.serviceuser (Id, Username, Mail, Age) VALUES (1, 'UserEight', 'usereight@mail.com', 100);
INSERT INTO userdb.serviceuser (Id, Username, Mail, Age) VALUES (2, 'UserOne', 'userone@mail.com', 18);
INSERT INTO userdb.serviceuser (Id, Username, Mail, Age) VALUES (3, 'UserTwo', 'usertwo@mail.com', 25);
INSERT INTO userdb.serviceuser (Id, Username, Mail, Age) VALUES (4, 'UserThree', 'userthree@mail.com', 30);
INSERT INTO userdb.serviceuser (Id, Username, Mail, Age) VALUES (5, 'UserFour', 'userfour@mail.com', 44);
INSERT INTO userdb.serviceuser (Id, Username, Mail, Age) VALUES (6, 'UserFive', 'userfive@mail.com', 50);
INSERT INTO userdb.serviceuser (Id, Username, Mail, Age) VALUES (7, 'UserSix', 'usersix@mail.com', 42);
INSERT INTO userdb.serviceuser (Id, Username, Mail, Age) VALUES (8, 'UserSeven', 'userseven@mail.com', 66);

