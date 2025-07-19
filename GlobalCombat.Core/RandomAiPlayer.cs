using System;
using System.Collections.Generic;
using System.Text;

namespace GlobalCombat.Core
{
	public class RandomAiPlayer
	{
		Game game;

		public RandomAiPlayer(Game game)
		{
			this.game = game;
		}

		public void Think()
		{
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();
			RandomAssignment();

			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();
			RandomAttack();

			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
			RandomTransfer();
		}

		Random random = new Random();

		void RandomAssignment()
		{
			Area area = game.Areas[random.Next(game.Areas.Count)];
			game.SetAssigned(area, 10);
		}

		void RandomTransfer()
		{
			Area area = game.Areas[random.Next(game.Areas.Count)];

			AreaInfo randomInbound = area.AreaInfo.Inbounds[random.Next(area.AreaInfo.Inbounds.Count)];
			Area target = game.Areas[randomInbound.Number - 1];

			game.SetTransfer(area, target, 20);
		}

		void RandomAttack()
		{
			Area area = game.Areas[random.Next(game.Areas.Count)];

			AreaInfo randomInbound = area.AreaInfo.Inbounds[random.Next(area.AreaInfo.Inbounds.Count)];
			Area target = game.Areas[randomInbound.Number - 1];

			game.SetAttack(area, target, 1000);
		}
	}
}
