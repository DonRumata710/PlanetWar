import React, { Component } from 'react'
import Page from '../components/page'
import { getSession, joinSession, leaveSession, startSession } from '../services/launchService'
import PropertyList from '../components/propertyList'
import LinkButton from '../components/linkButton';
import { setGameServiceAddress } from '../services/gameService'


class Session extends Component {
    constructor(props) {
        super(props);
        this.id = props.match.params.sessionId;
        this.sessionStarted = false;
        this.editPermission = this.props.location.state != null;
    }

    componentDidMount() {
        joinSession(this.id);
        this.loadData();
    }

    componentWillUnmount() {
        if (!this.sessionStarted)
            leaveSession();
    }

    loadData() {
        getSession(this.id).then((session) => {
            this.applyParameters(session.parameters)
        }).catch((reason) => {
            console.log("getSession failed ", reason)
        });
    }

    getParameters() {
        var res = {};
        this.parameters.forEach(element => {
            res[element.field] = element.currentValue
        });
        return res;
    }

    applyParameters(parameters) {
        this.parameters = [
            {
                field: "Name",
                name: "Name",
                type: "text",
                currentValue: parameters.name
            },
            {
                field: "Description",
                name: "Description",
                type: "text",
                currentValue: parameters.description
            },
            {
                field: "PlanetCount",
                name: "Planet count",
                type: "number",
                currentValue: parameters.planetCount
            },
            {
                field: "PlayerLimit",
                name: "Player limit",
                type: "number",
                currentValue: parameters.playerLimit
            },
            {
                field: "Size",
                name: "Map size",
                type: "number",
                currentValue: parameters.size
            }
        ];
        this.forceUpdate();
    }

    render() {
        if (this.parameters)
        {
            return (
                <Page>
                    <PropertyList properties={this.parameters} editPermission={this.editPermission} />
                    <LinkButton to={"/game/" + this.id} onClick={async () => {
                            this.sessionStarted = true;
                            setGameServiceAddress(await startSession(this.id));
                        }}>
                        Start game
                    </LinkButton>
                </Page>
            )
        }
        else
        {
            return (
                <Page />
            )
        }
    }
}

export default Session
