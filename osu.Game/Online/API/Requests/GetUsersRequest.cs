// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;

namespace osu.Game.Online.API.Requests
{
    public class GetUsersRequest : APIRequest<GetUsersResponse>
    {
        public readonly int[] UserIds;

        private const int max_ids_per_request = 50;

        public GetUsersRequest(int[] userIds)
        {
            if (userIds.Length > max_ids_per_request)
                throw new ArgumentException($"{nameof(GetUsersRequest)} calls only support up to {max_ids_per_request} IDs at once");

            UserIds = userIds;
        }

        protected override string Target => "users/?ids[]=" + string.Join("&ids[]=", UserIds);
    }
}
