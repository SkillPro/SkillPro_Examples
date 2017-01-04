#!/usr/bin/env python
# coding=utf-8

##############################################################################
#
# Copyright 2012-2016 SkillPro Consortium
#
# Author: Denis Štogl, email: denis.stogl@kit.edu
#
# Date of creation: 2014-2016
#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#
#
# This file is part of the SkillPro Framework. The SkillPro Framework
# is developed in the SkillPro project, funded by the European FP7
# programme (Grant Agreement 287733).
#
# The SkillPro Framework is free software: you can redistribute it and/or modify
# it under the terms of the GNU Lesser General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# The SkillPro Framework is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Lesser General Public License for more details.
#
# You should have received a copy of the GNU Lesser General Public License
# along with the SkillPro Framework.  If not, see <http://www.gnu.org/licenses/>.
##############################################################################

## @package skillpro_execution_engine
# @file simpleSEE
# @author Denis Štogl
##

import sys
import logging

from see.see import SkillExecutionEngine
from see.logging_module import LoggingModule
from see.threading_module import ThreadingModule


try:
    from IPython import embed
except ImportError:
    import code

    def embed():
        vars = globals()
        vars.update(locals())
        shell = code.InteractiveConsole(vars)
        shell.interact()


def main(args):

    logger = logging.getLogger("opcua.address_space")
    logger.setLevel(logging.WARN)
    logger = logging.getLogger("opcua.internal_server")
    logger.setLevel(logging.WARN)
    logger = logging.getLogger("opcua.binary_server_asyncio")
    logger.setLevel(logging.WARN)
    logger = logging.getLogger("opcua.uaprocessor")
    logger.setLevel(logging.WARN)
    logger = logging.getLogger("opcua.subscription_service")
    logger.setLevel(logging.WARN)

    logging.basicConfig(level=logging.INFO)

    see = SkillExecutionEngine(LoggingModule('see', 'DEBUG'),
                               ThreadingModule(),
                               './aml/SimpleSEE.aml',
                               '',
                               True,
                               'sbrc', 'Simple_SBRC',
                               False,
                               'state_machine', 'StateMachine_Fysom')

    try:
        embed()
    finally:
        see.shutdown()
        exit()


if __name__ == '__main__':
    main(sys.argv)

