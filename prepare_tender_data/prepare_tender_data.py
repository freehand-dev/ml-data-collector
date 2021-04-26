"""
Author: Anton Filonenko
"""

import sys
import aiohttp
import asyncio

from progress.bar import ShadyBar
import csv
from pprint import pprint
import re


tender_url = "https://dev.engineer-service.tv:58943/api/ml-data-collector"
opc_url = "https://public.api.openprocurement.org/api/2.5/tenders/"
tenders = []

path_to_file = sys.argv[1]
fieldnames = [
    "title",
    "is_good",
    "description",
    "id",
    "item_description",
    "item_clsf_description",
    "item_clsf_scheme",
    "item_clsf_id"
]


def write_tender_item_to_file(prep_data, item, csv_writer):

    item_clsf = item.get("classification",
                         {
                            "description": "",
                            "scheme": "",
                            "id": ""
                        })

    validate_and_norm_field(item, prep_data,
                            "description", "item_description")
    validate_and_norm_field(item_clsf, prep_data,
                            "description", "item_clsf_description")
    validate_and_norm_field(item_clsf, prep_data,
                            "scheme", "item_clsf_scheme")
    validate_and_norm_field(item_clsf, prep_data,
                            "id", "item_clsf_id")

    csv_writer.writerow(prep_data)


def validate_and_norm_field(old_dict, new_dict, old_key, new_key):

    value = old_dict.get(old_key, "")

    new_dict[new_key] = re.sub(r"\s", " ", value) if type(value) == str else value


async def write_tender_extended(csv_writer, tender, session):

    async with session.get(opc_url + tender['hash']) as res_i:
        tender_extended = await res_i.json()
        data = tender_extended["data"]

        norm_data = {}
        validate_and_norm_field(data, norm_data, "title", "title")
        validate_and_norm_field(tender, norm_data, "isGood", "is_good")
        validate_and_norm_field(data, norm_data, "description", "description")
        validate_and_norm_field(data, norm_data, "id", "id")

        items = data.get("items", [])
        for item in items:
            write_tender_item_to_file(norm_data.copy(), item, csv_writer)


async def write_tenders_to_file(path_to_file, tenders, session):

    with open(path_to_file, "w", encoding="utf-8", newline="") as csvfile:

        csv_writer = csv.DictWriter(csvfile,
                                    fieldnames=fieldnames,
                                    quoting=csv.QUOTE_NONNUMERIC)
        csv_writer.writeheader()

        for tender in ShadyBar("Fetching: ").iter(tenders):
            await write_tender_extended(csv_writer, tender, session)


async def fetch_tenders():

    async with aiohttp.ClientSession() as session:

        async with session.get(tender_url) as res:
            tenders = [{
                'hash': tnd['prozorroHash'],
                'isGood': tnd['isGood']
                } for tnd in await res.json()]

            await write_tenders_to_file(path_to_file, tenders, session)


if __name__ == '__main__':
    if sys.version_info[:2] == (3, 7):
        asyncio.set_event_loop_policy(asyncio.WindowsProactorEventLoopPolicy())

    loop = asyncio.get_event_loop()
    try:
        loop.run_until_complete(fetch_tenders())
        print("sleeping..")
        loop.run_until_complete(asyncio.sleep(2))
    finally:
        loop.close()
