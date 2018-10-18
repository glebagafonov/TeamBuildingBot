using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Domain.Entities;
using Bot.Infrastructure.Exceptions;

namespace Bot.Infrastructure.Helpers
{
    public static class GameHelper
    {
        public static (IEnumerable<Player> firstTeam, IEnumerable<Player> secondTeam) GetTeams(
            IEnumerable<Player> players, int playersPerTeam)
        {
            var listPlayers = players.ToList();
            if(listPlayers.Count() != playersPerTeam*2)
                throw new InvalidInputException();
            
            var combinations      = MathHelper.Combinations(listPlayers, playersPerTeam).Where( x => x.Count(y => y.IsGoalkeeper) == 1);
            var teamsCombinations = combinations.Select(x =>
            {
                var firstTeam  = x.ToList();
                var secondTeam = listPlayers.Where(y => firstTeam.All(z => z.Id != y.Id)).ToList();
                var skillSumFirstTeam = firstTeam.Sum(firstTeamPlayer => firstTeamPlayer.SkillValue);
                var skillSumSecondTeam = secondTeam.Sum(secondTeamPlayer => secondTeamPlayer.SkillValue);
                var difference = Math.Abs(skillSumFirstTeam - skillSumSecondTeam);
                
                return (firstTeam: firstTeam, secondTeam: secondTeam, difference: difference);
            });
            var result = teamsCombinations.OrderBy(x => x.difference).First();
            return (result.firstTeam, result.secondTeam);
        }
    }
}