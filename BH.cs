//bountyhunter.cs
function onStart(%mini)
{
	cancel(%mini.Timer);

	for(%i=0; %i < %mini.numMembers; %i++)
		%mini.member[%i].pickColor();
}

function onEnd(%mini)
{
	cancel(%mini.Timer);

	for(%i=0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		%cl.hitList = "";
		%cl.target = "";
		%cl.colorHex = "";
		%cl.colorRGB = "";
	}
}

function onJoin(%mini, %client)
{
	%mini = getMinigameFromObject(%client);
	%client.pickColor();
	%client.pickTarget();
	%client.pickHunter();

	for(%i=0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		if(%cl.hitList $= "")
			continue;

		%cl.hitList = setField(%cl.hitList,getFieldCount(%cl.hitList),%client);
	}

	if(%client.colorRGB !$= "" && %mini.colorNames && isObject(%client.player))
	{
		%client.player.setShapeNameColor(%client.colorRGB);
		%client.player.setNodeColor("chest",%client.colorRGB);
	}
}

function onLeave(%mini,%client)
{
	%mini = getMinigameFromObject(%client);
	for(%i=0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		if(%cl.target == %client)
		{
			%cl.target = "";
			%cl.schedule(750,pickTarget);
		}

		if(%cl.hitList $= "")
			continue;

		%cnt = getFieldCount(%cl.hitList);
		for(%f=0; %f < %cnt; %f++)
		{
			%fld = getField(%cl.hitList,%f);
			if(%fld == %client)
				%cl.hitList = removeField(%cl.hitList,%f);
		}
	}

	%client.hitList = "";
	%client.target = "";
	%client.colorHex = "";
	%client.colorRGB = "";
}

function postReset(%mini)
{
	cancel(%mini.BH_Timer);

	for(%i=0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		%cl.hitList = "";
		%cl.target = "";
		%cl.pickTarget();
	}

	%mini.Timer = schedule(30000,0,Tick,%mini);
}

function onSpawn(%mini,%client)
{
	if(%client.colorRGB !$= "" && %mini.colorNames && isObject(%client.player))
	{
		%client.player.setShapeNameColor(%client.colorRGB);
		%client.player.setNodeColor("chest",%client.colorRGB);
	}
}

function postDeath(%mini,%client,%obj,%killer,%type,%area)
{
	if(%killer == %client || !isObject(%killer))
		return;

	if(%killer.target != %client && %client.target != %killer)
	{
		messageClient(%killer,'',"\c0You may only kill your target. You receive a spawn penalty and lose 5 points otherwise.");
		%killer.bottomPrint("\c5You may only kill your target. You receive a spawn penalty and lose 5 points otherwise.",5);
		%killer.addDynamicRespawnTime(15000);
		
		if(isObject(%killer.player))
		{
					%killer.player.kill();
					%killer.incScore("-5");
		}
	}

	if(%killer.target == %client)
		%killer.incScore("-5");
	if(%client.target == %killer)
		%killer.incScore("-5");

	%killer.target = "";

	%killer.pickTarget();
	%client.pickHunter();
}

function isSpecialKill(%client,%sourceObject,%killer,%mini)
{
	if(%killer == %client || !isObject(%killer))
		return 0;

	if(!isObject(%killer.target) && !isObject(%client.target))
		return 0;

	if(%killer.target != %client && %client.target != %killer)
		return 0;

	if(%killer.target == %client)
	{
		%value = %value @ "\c3(Target)";
		%add = 1;
	}
	if(%client.target == %killer)
	{
		if(%value $= "")
			%value = %value @ "\c3(Hunter)";
		else
			%value = %value SPC "\c3(Hunter)";
		%add = 1;
	}

	if(%add)
		return 2 TAB %value;
	else
		return 0;
}
addSpecialDamageMsg("BountyHunter","%2%3%1 %4","");

function Tick(%mini)
{
	cancel(%mini.Timer);

	for(%i=0; %i < %mini.numMembers; %i++)
		serverCmdTarget(%mini.member[%i]);

	%mini.Timer = schedule(30000,0,Tick,%mini);
}

function GameConnection::pickTarget(%this)
{
	%mini = getMinigameFromObject(%this);
	if(%this.hitList $= "") //create a new hitlist
	{
		%cnt = 0;
		for(%i=0; %i < %mini.numMembers; %i++)
		{
			%m = %mini.member[%i];
			if(%this != %m)
			{
				%this.hitList = setField(%this.hitList,%cnt,%m);
				%cnt ++;
			}
		}
	}

	%r = getRandom(0,getFieldCount(%this.hitList)-1);
	%target = getField(%this.hitList,%r);
	%this.hitList = removeField(%this.hitList,%r);

	if(isObject(%target))
		%this.setTarget(%target);
}

function GameConnection::setTarget(%this,%target)
{
	if(!isObject(%target))
		return;

	%mini = getMinigameFromObject(%this);
	%targetMini = getMinigameFromObject(%target);
	if(%targetMini != %mini)
		return;

	if(%target == %this)
	{
		if(%mini.numMembers > 1)
			%this.pickTarget();
		return;
	}

	%this.target = %target;

	if(%mini.colorNames)
		%color = %this.target.colorHex;
	else
		%color = "ffff00";
	messageClient(%this,'',"\c0Your new target is <color:" @ %color @ ">" @ %this.target.getPlayerName() @ "\c5.");
	%this.bottomPrint("<just:center>\c0Your target is <color:" @ %color @ ">" @ %this.target.getPlayerName() @ "\c5.",5);
	schedule(getRandom(10000,20000),0,messageClient,%target,'',"\c0You are being hunted.");
}

function GameConnection::pickHunter(%this) //checks for a client without a target and sets them to the client
{
	%mini = %this.minigame;
	for(%i=0; %i < %mini.numMembers; %i++)
	{
		%cl = %mini.member[%i];
		if(%cl == %this)
			continue;
		if(isObject(%cl.target))
			continue;

		%hunter = %cl;
	}

	if(isObject(%hunter))
		%hunter.setTarget(%this);
}

function GameConnection::pickColor(%this)
{
	%mini = getMinigameFromObject(%this);

	%rgb = getRandom(50,255) SPC getRandom(50,255) SPC getRandom(50,255);
	%rgba = getWord(%rgb,0) / 255 SPC getWord(%rgb,1) / 255 SPC getWord(%rgb,2) / 255 SPC 1;
	%hex = Slayer_Support::RgbToHex(%rgb);

	%this.colorRGB = %rgba;
	%this.colorHex = %hex;

	if(%mini.colorNames && isObject(%client.player))
	{
		%client.player.setShapeNameColor(%rgba);
		%client.player.setNodeColor("chest",%rgba);
	}
}