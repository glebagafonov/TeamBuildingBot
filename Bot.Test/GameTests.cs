using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Bot.Domain.Entities;
using Bot.Infrastructure.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
namespace Bot.Test
{
    [TestClass]
    public class GameTests
    {
        
        
        [TestMethod]
        public void GenerateTeam()
        {
            var times = new List<(TimeSpan, int)>();
            
            times.Add((GenerateTime(2), 2));
            var count = Enumerable.Range(5, 7);
            foreach (var i in count)
            {
                times.Add((GenerateTime(i), i));
            }
        }

        private TimeSpan GenerateTime(int playersPerTeam)
        {
            var timer = new Stopwatch();
            List<Player> players;
            (IEnumerable<Player> firstTeam, IEnumerable<Player> secondTeam) team;
            players = GeneratePlayers(playersPerTeam * 2);
            timer.Start();
            team = GameHelper.GetTeams(players, playersPerTeam);
            timer.Stop();
            Assert.IsFalse(team.firstTeam.Concat(team.secondTeam).GroupBy(x => x.Id).Any(c => c.Count() > 1));
            return timer.Elapsed;
        }

        private List<Player> GeneratePlayers(int k)
        {
            Random gen = new Random((int)DateTime.Now.Ticks);
            var players = new List<Player>();

            foreach (var _ in Enumerable.Range(0, k))
            {
                var isGoalkeeper = players.Count(x => x.IsGoalkeeper) != 2 && gen.Next(100) < 50;
                players.Add(new Player()
                            {
                                Id = Guid.NewGuid(),
                                IsActive = true,
                                IsGoalkeeper = isGoalkeeper,
                                User = null,
                                SkillValue = isGoalkeeper ? gen.Next(90, 100) : gen.Next(40, 90),
                                ParticipationRatio = gen.Next(100)
                            });
            }

            return players;
        }
    }
}