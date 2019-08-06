import React, { useEffect } from "react";
import { connect } from "react-redux";
import * as authActions from "../../redux/actions/authActions";
import PropTypes from "prop-types";
import * as webSocketClient from "../../api/webSocketClient";

const Logout = ({ logout, history }) => {
    useEffect(() => {
        logout();
        webSocketClient.closeAllOpenConnections();
        history.push("/");
    });

    return <div />;
};

Logout.propTypes = {
    logout: PropTypes.func.isRequired,
    history: PropTypes.object.isRequired
};

const mapDispatchToProps = {
    logout: authActions.logout
};

export default connect(
    null,
    mapDispatchToProps
)(Logout);