﻿using Lime.Protocol;
using Lollapalooza.Services.Extension;
using Lollapalooza.Services.Interface;
using Lollapalooza.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.Broadcast;

namespace Lollapalooza.Services.Service
{
    public class ScheduleExtensionService : IScheduleExtensionService
    {
        private readonly IBroadcastExtension _broadcastExtension;
        public ScheduleExtensionService(IBroadcastExtension broadcastExtension)
        {
            _broadcastExtension = broadcastExtension;
        }

        /// <summary>
        /// Create distribution list and insert user inside
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <param name="show"></param>
        /// <param name="showRemember"></param>
        /// <param name="timeMinutesToAlert"></param>
        public async Task InsertUserAtDistributionListAsync(string userIdentifier, Show show, int timeMinutesToAlert)
        {
            var distributionListName = GetDistributionListName(show, timeMinutesToAlert);
            await _broadcastExtension.CreateDistributionListAsync(distributionListName);
            await _broadcastExtension.AddRecipientAsync(distributionListName, Identity.Parse(userIdentifier));
        }

        /// <summary>
        /// Validate user distribution list, based on user choices
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <param name="oldUserSchedules"></param>
        /// <param name="showRemember"></param>
        /// <param name="timeMinutesToAlert"></param>
        /// <returns></returns>
        public async Task ManageUserDistributionListAsync(string userIdentifier, List<UserSchedule> oldUserSchedules, bool showRemember, int timeMinutesToAlert)
        {
            string distributionListName = string.Empty;
            foreach (var item in oldUserSchedules)
            {
                if (!showRemember)
                {
                    //remove user from all distribution List
                    distributionListName = GetDistributionListName(item.Show, item.TimeMinutesToAlert);
                    await _broadcastExtension.DeleteRecipientAsync(distributionListName, Identity.Parse(userIdentifier));
                }
                else
                {
                    //remove from old distribution list
                    distributionListName = GetDistributionListName(item.Show, item.TimeMinutesToAlert);
                    await _broadcastExtension.DeleteRecipientAsync(distributionListName, Identity.Parse(userIdentifier));

                    //create and insert on the new list
                    distributionListName = GetDistributionListName(item.Show, timeMinutesToAlert);
                    await _broadcastExtension.CreateDistributionListAsync(distributionListName);

                    await _broadcastExtension.AddRecipientAsync(distributionListName, Identity.Parse(userIdentifier));
                }
            }
        }

        /// <summary>
        /// Remove user from distribution list
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <param name="show"></param>
        /// <param name="timeMinutesToAlert"></param>
        public async Task RemoveUserFromDistributionListAsync(string userIdentifier, Show show, int timeMinutesToAlert)
        {
            var distributionListName = GetDistributionListName(show, timeMinutesToAlert);
            await _broadcastExtension.DeleteRecipientAsync(distributionListName, Identity.Parse(userIdentifier));
        }

        /// <summary>
        /// Generate Distribution List Name
        /// </summary>
        /// <param name="show"></param>
        /// <param name="timeMinutesToAlert"></param>
        /// <returns></returns>
        private string GetDistributionListName(Show show, int timeMinutesToAlert) => $"{show.Day.RemoveSpecialCharacter()}_{show.StartTime.RemoveSpecialCharacter()}_{timeMinutesToAlert}_{show.ShowId}_{show.Band.RemoveSpecialCharacter()}";
    }
}
