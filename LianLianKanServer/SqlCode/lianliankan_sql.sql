USE mydatabase;

CREATE TABLE lianliankan_user
(
	id int NOT NULL AUTO_INCREMENT,
    user_account char(24) NOT NULL,
    user_password char(32) NOT NULL,
    user_name varchar(30) NOT NULL,
    user_head_path varchar(256) DEFAULT 'no image', 
    user_introduce varchar(128) NULL, 
    PRIMARY KEY(id)
)ENGINE=InnoDB;

DROP TABLE lianliankan_user;

INSERT INTO lianliankan_user(user_account, user_password, user_name, user_introduce)
 VALUES('root', 'root123', 'root', 'The account of manager, The first account as well');
INSERT INTO lianliankan_user(user_account, user_password, user_name, user_introduce)
 VALUES('user1', 'user123', 'user1', 'It\'s user1');
INSERT INTO lianliankan_user(user_account, user_password, user_name, user_introduce)
 VALUES('user2', 'user123', 'user2', 'It\'s user2');
INSERT INTO lianliankan_user(user_account, user_password, user_name, user_introduce)
 VALUES('user3', 'user123', 'user3', 'It\'s user3');
SELECT * FROM lianliankan_user;


CREATE TABLE lianliankan_game_record
(
id int NOT NULL,
play_time datetime,
game_time varchar(16),
FOREIGN KEY(id) REFERENCES lianliankan_user(id)
) ENGINE = InnoDB;

DROP TABLE lianliankan_game_record;

INSERT INTO lianliankan_game_record(id, play_time, game_time)
 VALUES(1, now(), '11:12:123456');
INSERT INTO lianliankan_game_record(id, play_time, game_time)
 VALUES(2, now(), '11:12:123456');
INSERT INTO lianliankan_game_record(id, play_time, game_time)
 VALUES(3, now(), '11:12:123456');
INSERT INTO lianliankan_game_record(id, play_time, game_time)
 VALUES(1, now(), '11:12:123456');
INSERT INTO lianliankan_game_record(id, play_time, game_time)
 VALUES(3, now(), '11:12:123456');
 SELECT * FROM lianliankan_game_record;
 
 
CALL insert_into_lianliankan_game_record(3, now(), '11:12:333333');
 
 
SELECT * FROM lianliankan_game_record WHERE id = 3 ORDER BY play_time ASC;
DELETE FROM lianliankan_game_record WHERE '2020-11-28 14:50:00' >= play_time AND id = 1;
SELECT MAX(play_time) FROM (SELECT play_time FROM lianliankan_game_record WHERE id = 3
ORDER BY play_time ASC LIMIT 2) AS temp;
SELECT play_time FROM lianliankan_game_record WHERE id = 3
ORDER BY play_time ASC LIMIT 2;

DROP PROCEDURE insert_into_lianliankan_game_record;
DROP TRIGGER before_insert_int_lianliankan_game_record;

DELIMITER //
#必须用插入函数记录游戏记录
CREATE PROCEDURE insert_into_lianliankan_game_record
(
IN in_id INT,
IN in_play_time DATETIME,
IN in_game_time varchar(16)
)
BEGIN
DECLARE limitTime DATETIME;
DECLARE delNum INT;
DECLARE sum INT;

/*插入*/
#先插入带有密码的行，通过触发器before_insert_int_lianliankan_game_record
INSERT INTO lianliankan_game_record(id, play_time, game_time)
 VALUES(in_id, in_play_time, 'daf12e324fvxc');
 #在更新game_time为正确值
 UPDATE lianliankan_game_record SET game_time = in_game_time 
 WHERE play_time = in_play_time AND id = in_id;
 
/*在检查是否超过30个记录*/
#获取总行数
SELECT COUNT(id) FROM lianliankan_game_record WHERE in_id = id INTO sum;
SET delNum = sum -30;
IF delNum > 0 THEN
#获取需要删除行的时间的上限
SELECT MAX(play_time) FROM 
(SELECT play_time FROM lianliankan_game_record WHERE id = in_id
ORDER BY play_time ASC LIMIT delNum) AS temp_list INTO limitTime;
#将超过的删除
DELETE FROM lianliankan_game_record WHERE limitTime >= play_time AND id = in_id;
END IF;

END//

#创建 阻止INSERT的触发器
CREATE TRIGGER before_insert_int_lianliankan_game_record
BEFORE INSERT ON lianliankan_game_record FOR EACH ROW
BEGIN
IF NEW.game_time != 'daf12e324fvxc' THEN
	SIGNAL SQLSTATE 'HY000' SET MESSAGE_TEXT = 'MUST use procedure insert_into_lianliankan_game_record to INSERT INTO lianliankan_game_record';
END IF;
END//
   
DELIMITER ;
