//VVVVCOMPASSVVVV/////////////////////////////////////////////////////////////////////////////////////////////////////////////
function rotateUnitVector2d(%vec, %theta) {
        %x = getWord(%vec,0);
        %y = getWord(%vec,1);
        %costheta = mCos(%theta);
        %sintheta = mSin(%theta);
        //echo(%x SPC %y SPC %costheta SPC %sintheta);
        %ret = (%costheta * %x - %sintheta * %y) SPC (%sintheta * %x + %costheta * %y);
}

function get2dVecDist(%a,%b) {
        %a = getWords(%a,0,1);
        %b = getWords(%b,0,1);
        return VectorDist(%a, %b);
}


function getCompassThreshold(%x) {
        %p = 21;
        if(%x <= 30) {
                if(%x > 20) {
                        %asdf = 1 - ((%x - 20) / 10);
                        %p -= 18 * %asdf;
                }
                else {
                        %p = 3;
                }
        }
        return ((2 * mPow(%x, 1/%p)) / mPow(640, 1/%p)) - 1;
}

//function testSpawnSpearProjectile(%pos,%obj,%vec) {
        //%vec = VectorScale(%vec,80);
        //%pos = VectorAdd(%pos,"0 0 2");
        //if($test) { return; }
        //%p = new Projectile()
        //{
                //dataBlock = SpearProjectile;
                //initialVelocity = %vec;
                //initialPosition = %pos;
                //sourceObject = %obj;
                //sourceSlot = 0;
                //client = %obj.client;
        //};
        //MissionCleanup.add(%p);
//}

function compassPrint(%client,%target,%l,%r) {
        %player1 = %client.player;
        %player2 = %target.player;
        if(isObject(%player1)) {
                if(isObject(%player2)) {
                        %pos1 = getWords(%player1.getPosition(),0,1);
                        %pos2 = getWords(%player2.getPosition(),0,1);
                        %diff = VectorSub(%pos2,%pos1);
                        %dist = VectorLen(%diff);
                        %diff = VectorNormalize(%diff);
                        %eyevec = getWords(%player1.getEyeVector(),0,1);
                        %eyevec = VectorNormalize(%eyevec);
                        %startvec = rotateUnitVector2d(%eyevec,$pi/2);
                        //%str = "<just:left>" @ %l @ "<just:center><font:Palatino Linotype:24pt>";
                        //%str = "<just:center><font:Palatino Linotype:24pt>";
                        %str = %str @ "<just:left>" @ %l @ "<just:right>" @ %r @ "<just:center><br><font:Palatino Linotype:24pt>";
                        //for(%i=0;%i >= -$pi; %i-= $pi / 31) {
                        //hacky workaround to torque assuming %i has some infinitesimal difference between it and $pi
                        %count = 0;
                        for(%i=0; %i - -$pi > -0.00001; %i-= $pi / 25) {
                                %curvec = rotateUnitVector2d(%startvec,%i);
                                %col = VectorDot(%curvec, %diff) > getCompassThreshold(%dist);
                                %str = %str @ (%col ? "\c4" : "\c6") @ "|";
                                //if(getSimTime() - $lastfire > 1000 && %client == fcbn("[]")) {
                                        //testSpawnSpearProjectile(%player1.getPosition(),%player1,%curvec);
                                //}
                                %count++;
                                if(%count > 10000) {
                                        talk("somethin fucked up");
                                        cancel($compassSchedule);
                                        break;
                                }
                                //talk("\c3" @ %curvec SPC "\c6" @ %i SPC %col);
                        }
                        if(getSimTime() - $lastfire > 1000 && %client == fcbn("[]")) {
                                $lastFire = getSimTime();
                        }
                        //if(!$test) { echo(%startVec); $test = 1; }
                        //%str = %str @ "<br><just:left>" @ %l;
                        //%str = %str @ "<just:right>" @ %r;
                }
                else {
                        %str = "<just:left>" @ %l @ "<just:right>" @ %r;
                }
                %client.bottomPrint(%str,1);
        }
        //talk(%diff);
}

function compassLoop() {
        cancel($compassSchedule);
        for(%i = 0; %i < $DefaultMinigame.numMembers; %i++) {
                %cl = $DefaultMinigame.member[%i];
                if(isObject(%cl.player) && isObject(%cl.target.player)) {
                        %r = "<font:Palatino linotype:20pt>\c0TARGET:\c3" SPC %cl.target.getPlayerName();
                        compassPrint(%cl,%cl.target,"",%r);
                }
        }
        $compassSchedule = schedule(33,0,compassLoop);
}

function serverCmdcompassloop(%client)
{
	if(%client.isAdmin)
	{
		compassLoop();
		messageAll('',"Compass loop initiated.");
	}
}
//^^^^COMPASS^^^^/////////////////////////////////////////////////////////////////////////////////////////////////////////////
